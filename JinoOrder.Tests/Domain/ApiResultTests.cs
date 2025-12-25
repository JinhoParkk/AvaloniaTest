using JinoOrder.Domain.Common;
using FluentAssertions;
using Xunit;

namespace JinoOrder.Tests.Domain;

public class ApiResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Arrange & Act
        var result = ApiResult<string>.Success("test data");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Data.Should().Be("test data");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange & Act
        var result = ApiResult<string>.Failure("error message", 500);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("error message");
        result.Error.StatusCode.Should().Be(500);
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedResult()
    {
        // Arrange & Act
        var result = ApiResult<string>.Unauthorized();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(401);
        result.Error.IsUnauthorized.Should().BeTrue();
    }
}
