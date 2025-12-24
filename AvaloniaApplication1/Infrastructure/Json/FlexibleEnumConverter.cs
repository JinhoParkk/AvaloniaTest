using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaloniaApplication1.Infrastructure.Json;

/// <summary>
/// 알 수 없는 enum 값을 [UnknownEnumValue]로 마킹된 fallback 값으로 변환하는 JsonConverter.
/// Kotlin/Java의 FlexibleEnum 패턴과 유사하게 동작.
///
/// 서버에서 새로운 enum 값이 추가되어도 클라이언트가 크래시하지 않고
/// Unknown 값으로 graceful하게 처리됨.
/// </summary>
public class FlexibleEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Nullable<Enum>도 지원
        if (typeToConvert.IsEnum)
            return true;

        if (typeToConvert.IsGenericType &&
            typeToConvert.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
            return underlyingType?.IsEnum == true;
        }

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type enumType;
        bool isNullable = false;

        if (typeToConvert.IsEnum)
        {
            enumType = typeToConvert;
        }
        else
        {
            // Nullable<Enum>
            enumType = Nullable.GetUnderlyingType(typeToConvert)!;
            isNullable = true;
        }

        var converterType = isNullable
            ? typeof(NullableFlexibleEnumConverter<>).MakeGenericType(enumType)
            : typeof(FlexibleEnumConverter<>).MakeGenericType(enumType);

        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}

/// <summary>
/// enum 타입별 메타데이터 캐시
/// </summary>
internal static class EnumMetadataCache<T> where T : struct, Enum
{
    private static readonly Lazy<EnumMetadata> LazyMetadata = new(BuildMetadata);

    public static EnumMetadata Metadata => LazyMetadata.Value;

    private static EnumMetadata BuildMetadata()
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

        T? unknownValue = null;
        var nameToValue = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        var valueToName = new Dictionary<T, string>();

        foreach (var field in fields)
        {
            var value = (T)field.GetValue(null)!;
            var name = field.Name;

            // Unknown 값 찾기
            if (field.GetCustomAttribute<UnknownEnumValueAttribute>() != null)
            {
                unknownValue = value;
            }

            // 이름 -> 값 매핑 (대소문자 무시)
            nameToValue[name] = value;

            // snake_case 변환도 추가
            var snakeCase = ConvertToSnakeCase(name);
            if (!nameToValue.ContainsKey(snakeCase))
            {
                nameToValue[snakeCase] = value;
            }

            // 값 -> camelCase 이름 매핑 (직렬화용)
            var camelCase = char.ToLowerInvariant(name[0]) + name.Substring(1);
            valueToName[value] = camelCase;
        }

        return new EnumMetadata(unknownValue, nameToValue, valueToName);
    }

    private static string ConvertToSnakeCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < pascalCase.Length; i++)
        {
            var c = pascalCase[i];
            if (i > 0 && char.IsUpper(c))
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    internal class EnumMetadata
    {
        public T? UnknownValue { get; }
        public Dictionary<string, T> NameToValue { get; }
        public Dictionary<T, string> ValueToName { get; }

        public EnumMetadata(
            T? unknownValue,
            Dictionary<string, T> nameToValue,
            Dictionary<T, string> valueToName)
        {
            UnknownValue = unknownValue;
            NameToValue = nameToValue;
            ValueToName = valueToName;
        }
    }
}

/// <summary>
/// 특정 enum 타입을 위한 FlexibleEnumConverter 구현.
/// </summary>
/// <typeparam name="T">대상 enum 타입</typeparam>
public class FlexibleEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var metadata = EnumMetadataCache<T>.Metadata;

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                return ParseFromString(stringValue, metadata);

            case JsonTokenType.Number:
                var intValue = reader.GetInt32();
                return ParseFromNumber(intValue, metadata);

            case JsonTokenType.Null:
                return metadata.UnknownValue ?? default;

            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} for enum {typeof(T).Name}");
        }
    }

    private static T ParseFromString(string? value, EnumMetadataCache<T>.EnumMetadata metadata)
    {
        if (string.IsNullOrEmpty(value))
        {
            return metadata.UnknownValue ?? default;
        }

        // 캐시된 매핑에서 찾기 (대소문자 무시, snake_case 포함)
        if (metadata.NameToValue.TryGetValue(value, out var result))
        {
            return result;
        }

        // snake_case -> PascalCase 변환 시도
        var pascalCase = ConvertToPascalCase(value);
        if (metadata.NameToValue.TryGetValue(pascalCase, out result))
        {
            return result;
        }

        // 매칭 실패 시 Unknown fallback
        return metadata.UnknownValue ?? default;
    }

    private static T ParseFromNumber(int value, EnumMetadataCache<T>.EnumMetadata metadata)
    {
        if (Enum.IsDefined(typeof(T), value))
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        // 정의되지 않은 숫자값 -> Unknown fallback
        return metadata.UnknownValue ?? default;
    }

    private static string ConvertToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;

        var parts = snakeCase.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1).ToLowerInvariant();
            }
        }
        return string.Join("", parts);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var metadata = EnumMetadataCache<T>.Metadata;

        if (metadata.ValueToName.TryGetValue(value, out var name))
        {
            writer.WriteStringValue(name);
        }
        else
        {
            // fallback: 기본 ToString
            writer.WriteStringValue(value.ToString());
        }
    }
}

/// <summary>
/// Nullable enum을 위한 컨버터
/// </summary>
public class NullableFlexibleEnumConverter<T> : JsonConverter<T?> where T : struct, Enum
{
    private readonly FlexibleEnumConverter<T> _innerConverter = new();

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return _innerConverter.Read(ref reader, typeof(T), options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            _innerConverter.Write(writer, value.Value, options);
        }
    }
}
