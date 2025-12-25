using System.Text.Json;
using JinoOrder.Infrastructure.Json;
using FluentAssertions;
using Xunit;

namespace JinoOrder.Tests.Infrastructure.Json;

public class FlexibleEnumConverterTests
{
    private readonly JsonSerializerOptions _options;

    public FlexibleEnumConverterTests()
    {
        _options = JsonSerializerOptionsExtensions.CreateApiOptions();
    }

    #region Test Enums

    public enum OrderStatus
    {
        [UnknownEnumValue]
        Unknown = 0,
        Pending,
        Processing,
        Completed,
        Cancelled
    }

    public enum PaymentType
    {
        CreditCard,
        Cash,
        [UnknownEnumValue]
        Other
    }

    public enum NoFallbackEnum
    {
        Value1,
        Value2
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentType Payment { get; set; }
    }

    public class NullableOrderDto
    {
        public int Id { get; set; }
        public OrderStatus? Status { get; set; }
        public PaymentType? Payment { get; set; }
    }

    #endregion

    [Theory]
    [InlineData("\"pending\"", OrderStatus.Pending)]
    [InlineData("\"Pending\"", OrderStatus.Pending)]
    [InlineData("\"PENDING\"", OrderStatus.Pending)]
    [InlineData("\"processing\"", OrderStatus.Processing)]
    [InlineData("\"completed\"", OrderStatus.Completed)]
    public void Read_KnownStringValue_ShouldParseCorrectly(string json, OrderStatus expected)
    {
        // Act
        var result = JsonSerializer.Deserialize<OrderStatus>(json, _options);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"future_status\"")]
    [InlineData("\"newValue\"")]
    [InlineData("\"\"")]
    [InlineData("null")]
    public void Read_UnknownStringValue_ShouldReturnUnknownFallback(string json)
    {
        // Act
        var result = JsonSerializer.Deserialize<OrderStatus>(json, _options);

        // Assert
        result.Should().Be(OrderStatus.Unknown);
    }

    [Fact]
    public void Read_SnakeCaseValue_ShouldConvertToPascalCase()
    {
        // Arrange
        var json = "\"credit_card\"";

        // Act
        var result = JsonSerializer.Deserialize<PaymentType>(json, _options);

        // Assert
        result.Should().Be(PaymentType.CreditCard);
    }

    [Theory]
    [InlineData(1, OrderStatus.Pending)]
    [InlineData(2, OrderStatus.Processing)]
    [InlineData(3, OrderStatus.Completed)]
    public void Read_KnownIntValue_ShouldParseCorrectly(int value, OrderStatus expected)
    {
        // Arrange
        var json = value.ToString();

        // Act
        var result = JsonSerializer.Deserialize<OrderStatus>(json, _options);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Read_UnknownIntValue_ShouldReturnUnknownFallback(int value)
    {
        // Arrange
        var json = value.ToString();

        // Act
        var result = JsonSerializer.Deserialize<OrderStatus>(json, _options);

        // Assert
        result.Should().Be(OrderStatus.Unknown);
    }

    [Fact]
    public void Read_ComplexObject_ShouldHandleEnumsCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": 123,
            "status": "processing",
            "payment": "creditCard"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<OrderDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Status.Should().Be(OrderStatus.Processing);
        result.Payment.Should().Be(PaymentType.CreditCard);
    }

    [Fact]
    public void Read_ComplexObjectWithUnknownEnum_ShouldUseFallback()
    {
        // Arrange - 서버에서 새로운 status가 추가된 경우
        var json = """
        {
            "id": 456,
            "status": "shipped",
            "payment": "crypto"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<OrderDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(456);
        result.Status.Should().Be(OrderStatus.Unknown);
        result.Payment.Should().Be(PaymentType.Other);
    }

    [Fact]
    public void Read_EnumWithoutFallback_ShouldReturnDefault()
    {
        // Arrange - UnknownEnumValue가 없는 enum
        var json = "\"unknownValue\"";

        // Act
        var result = JsonSerializer.Deserialize<NoFallbackEnum>(json, _options);

        // Assert - default(NoFallbackEnum) = Value1 (0)
        result.Should().Be(NoFallbackEnum.Value1);
    }

    [Fact]
    public void Write_ShouldSerializeAsCamelCase()
    {
        // Arrange
        var value = OrderStatus.Processing;

        // Act
        var json = JsonSerializer.Serialize(value, _options);

        // Assert
        json.Should().Be("\"processing\"");
    }

    [Fact]
    public void Write_ComplexObject_ShouldSerializeEnumsAsCamelCase()
    {
        // Arrange
        var order = new OrderDto
        {
            Id = 789,
            Status = OrderStatus.Completed,
            Payment = PaymentType.CreditCard
        };

        // Act
        var json = JsonSerializer.Serialize(order, _options);

        // Assert
        json.Should().Contain("\"status\":\"completed\"");
        json.Should().Contain("\"payment\":\"creditCard\"");
    }

    [Fact]
    public void RoundTrip_ShouldPreserveValue()
    {
        // Arrange
        var original = OrderStatus.Processing;

        // Act
        var json = JsonSerializer.Serialize(original, _options);
        var result = JsonSerializer.Deserialize<OrderStatus>(json, _options);

        // Assert
        result.Should().Be(original);
    }

    #region Nullable Enum Tests

    [Fact]
    public void Read_NullableEnum_WithValue_ShouldParse()
    {
        // Arrange
        var json = "\"processing\"";

        // Act
        var result = JsonSerializer.Deserialize<OrderStatus?>(json, _options);

        // Assert
        result.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Read_NullableEnum_WithNull_ShouldReturnNull()
    {
        // Arrange
        var json = "null";

        // Act
        var result = JsonSerializer.Deserialize<OrderStatus?>(json, _options);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Read_NullableEnum_UnknownValue_ShouldUseFallback()
    {
        // Arrange
        var json = "\"future_status\"";

        // Act
        var result = JsonSerializer.Deserialize<OrderStatus?>(json, _options);

        // Assert
        result.Should().Be(OrderStatus.Unknown);
    }

    [Fact]
    public void Read_ComplexObjectWithNullableEnums_ShouldHandleNull()
    {
        // Arrange
        var json = """
        {
            "id": 123,
            "status": null,
            "payment": "cash"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<NullableOrderDto>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(123);
        result.Status.Should().BeNull();
        result.Payment.Should().Be(PaymentType.Cash);
    }

    [Fact]
    public void Write_NullableEnum_WithValue_ShouldSerialize()
    {
        // Arrange
        OrderStatus? value = OrderStatus.Completed;

        // Act
        var json = JsonSerializer.Serialize(value, _options);

        // Assert
        json.Should().Be("\"completed\"");
    }

    [Fact]
    public void Write_NullableEnum_WithNull_ShouldSerializeAsNull()
    {
        // Arrange
        OrderStatus? value = null;

        // Act
        var json = JsonSerializer.Serialize(value, _options);

        // Assert
        json.Should().Be("null");
    }

    #endregion
}
