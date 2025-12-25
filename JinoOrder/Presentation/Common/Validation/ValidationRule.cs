namespace JinoOrder.Presentation.Common.Validation;

/// <summary>
/// 검증 규칙 인터페이스
/// </summary>
/// <typeparam name="T">검증할 값의 타입</typeparam>
public interface IValidationRule<in T>
{
    /// <summary>
    /// 검증 오류 메시지
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// 값 검증 수행
    /// </summary>
    bool Validate(T value);
}

/// <summary>
/// 필수 입력 검증 규칙
/// </summary>
public class RequiredRule : IValidationRule<string>
{
    public string ErrorMessage { get; }

    public RequiredRule(string errorMessage = "필수 입력 항목입니다.")
    {
        ErrorMessage = errorMessage;
    }

    public bool Validate(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}

/// <summary>
/// 최소 길이 검증 규칙
/// </summary>
public class MinLengthRule : IValidationRule<string>
{
    private readonly int _minLength;
    public string ErrorMessage { get; }

    public MinLengthRule(int minLength, string? errorMessage = null)
    {
        _minLength = minLength;
        ErrorMessage = errorMessage ?? $"최소 {minLength}자 이상 입력해주세요.";
    }

    public bool Validate(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length >= _minLength;
    }
}

/// <summary>
/// 최대 길이 검증 규칙
/// </summary>
public class MaxLengthRule : IValidationRule<string>
{
    private readonly int _maxLength;
    public string ErrorMessage { get; }

    public MaxLengthRule(int maxLength, string? errorMessage = null)
    {
        _maxLength = maxLength;
        ErrorMessage = errorMessage ?? $"최대 {maxLength}자까지 입력 가능합니다.";
    }

    public bool Validate(string value)
    {
        return string.IsNullOrEmpty(value) || value.Length <= _maxLength;
    }
}

/// <summary>
/// 범위 검증 규칙
/// </summary>
public class RangeRule<T> : IValidationRule<T> where T : IComparable<T>
{
    private readonly T _min;
    private readonly T _max;
    public string ErrorMessage { get; }

    public RangeRule(T min, T max, string? errorMessage = null)
    {
        _min = min;
        _max = max;
        ErrorMessage = errorMessage ?? $"{min}에서 {max} 사이의 값을 입력해주세요.";
    }

    public bool Validate(T value)
    {
        return value.CompareTo(_min) >= 0 && value.CompareTo(_max) <= 0;
    }
}

/// <summary>
/// 이메일 형식 검증 규칙
/// </summary>
public class EmailRule : IValidationRule<string>
{
    public string ErrorMessage { get; }

    public EmailRule(string errorMessage = "올바른 이메일 형식이 아닙니다.")
    {
        ErrorMessage = errorMessage;
    }

    public bool Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true; // Required 규칙과 분리

        return value.Contains('@') && value.Contains('.');
    }
}

/// <summary>
/// 사용자 정의 검증 규칙
/// </summary>
public class CustomRule<T> : IValidationRule<T>
{
    private readonly Func<T, bool> _validate;
    public string ErrorMessage { get; }

    public CustomRule(Func<T, bool> validate, string errorMessage)
    {
        _validate = validate;
        ErrorMessage = errorMessage;
    }

    public bool Validate(T value)
    {
        return _validate(value);
    }
}
