using System;

namespace JinoOrder.Infrastructure.Json;

/// <summary>
/// enum에서 알 수 없는 값이 들어왔을 때 fallback으로 사용할 값을 지정하는 어트리뷰트.
/// 서버에서 새로운 enum 값이 추가되어도 클라이언트가 크래시하지 않음.
/// </summary>
/// <example>
/// public enum OrderStatus
/// {
///     [UnknownEnumValue]
///     Unknown = 0,
///     Pending,
///     Processing,
///     Completed
/// }
/// </example>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class UnknownEnumValueAttribute : Attribute
{
}
