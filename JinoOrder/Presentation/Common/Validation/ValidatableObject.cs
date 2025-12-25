using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JinoOrder.Presentation.Common.Validation;

/// <summary>
/// 검증 가능한 객체 래퍼
/// </summary>
/// <typeparam name="T">값의 타입</typeparam>
public partial class ValidatableObject<T> : ObservableObject
{
    private readonly List<IValidationRule<T>> _validations = new();

    [ObservableProperty]
    private T _value = default!;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isValid = true;

    /// <summary>
    /// 검증 규칙 목록
    /// </summary>
    public IReadOnlyList<IValidationRule<T>> Validations => _validations.AsReadOnly();

    /// <summary>
    /// 검증 오류가 있는지 여부
    /// </summary>
    public bool HasError => !IsValid;

    public ValidatableObject()
    {
    }

    public ValidatableObject(T initialValue)
    {
        _value = initialValue;
    }

    /// <summary>
    /// 검증 규칙 추가
    /// </summary>
    public ValidatableObject<T> AddRule(IValidationRule<T> rule)
    {
        _validations.Add(rule);
        return this;
    }

    /// <summary>
    /// 필수 입력 규칙 추가 (string 타입 전용)
    /// </summary>
    public ValidatableObject<T> Required(string errorMessage = "필수 입력 항목입니다.")
    {
        if (this is ValidatableObject<string> stringObj)
        {
            _validations.Add((IValidationRule<T>)(object)new RequiredRule(errorMessage));
        }
        return this;
    }

    /// <summary>
    /// 검증 수행
    /// </summary>
    /// <returns>유효 여부</returns>
    public bool Validate()
    {
        ErrorMessage = null;
        IsValid = true;

        foreach (var rule in _validations)
        {
            if (!rule.Validate(Value))
            {
                ErrorMessage = rule.ErrorMessage;
                IsValid = false;
                OnPropertyChanged(nameof(HasError));
                return false;
            }
        }

        OnPropertyChanged(nameof(HasError));
        return true;
    }

    /// <summary>
    /// 오류 상태 초기화
    /// </summary>
    public void ClearErrors()
    {
        ErrorMessage = null;
        IsValid = true;
        OnPropertyChanged(nameof(HasError));
    }

    /// <summary>
    /// 값 설정 후 즉시 검증
    /// </summary>
    public bool SetAndValidate(T value)
    {
        Value = value;
        return Validate();
    }

    /// <summary>
    /// 암시적 변환: T -> ValidatableObject<T>
    /// </summary>
    public static implicit operator T(ValidatableObject<T> obj) => obj.Value;

    /// <summary>
    /// 암시적 변환: ValidatableObject<T> -> T
    /// </summary>
    public static implicit operator ValidatableObject<T>(T value) => new(value);

    public override string ToString() => Value?.ToString() ?? string.Empty;
}

/// <summary>
/// ValidatableObject 확장 메서드
/// </summary>
public static class ValidatableObjectExtensions
{
    /// <summary>
    /// 여러 ValidatableObject 동시 검증
    /// </summary>
    public static bool ValidateAll(params object[] validatables)
    {
        var allValid = true;

        foreach (var obj in validatables)
        {
            var validateMethod = obj.GetType().GetMethod("Validate");
            if (validateMethod != null)
            {
                var result = (bool)(validateMethod.Invoke(obj, null) ?? false);
                if (!result)
                {
                    allValid = false;
                }
            }
        }

        return allValid;
    }

    /// <summary>
    /// 첫 번째 오류 메시지 반환
    /// </summary>
    public static string? GetFirstError(params object[] validatables)
    {
        foreach (var obj in validatables)
        {
            var type = obj.GetType();
            var isValidProp = type.GetProperty("IsValid");
            var errorMessageProp = type.GetProperty("ErrorMessage");

            if (isValidProp != null && errorMessageProp != null)
            {
                var isValid = (bool)(isValidProp.GetValue(obj) ?? true);
                if (!isValid)
                {
                    return errorMessageProp.GetValue(obj) as string;
                }
            }
        }

        return null;
    }
}
