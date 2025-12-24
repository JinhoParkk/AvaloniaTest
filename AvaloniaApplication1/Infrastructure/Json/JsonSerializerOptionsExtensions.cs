using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaloniaApplication1.Infrastructure.Json;

/// <summary>
/// JsonSerializerOptions 확장 메서드
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// API 통신에 사용할 기본 JsonSerializerOptions 생성.
    /// FlexibleEnumConverter가 포함되어 알 수 없는 enum 값도 안전하게 처리됨.
    /// </summary>
    public static JsonSerializerOptions CreateApiOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new FlexibleEnumConverterFactory());

        return options;
    }

    /// <summary>
    /// 기존 옵션에 FlexibleEnumConverter 추가
    /// </summary>
    public static JsonSerializerOptions WithFlexibleEnums(this JsonSerializerOptions options)
    {
        options.Converters.Add(new FlexibleEnumConverterFactory());
        return options;
    }
}
