using System;
using JinoOrder.Application.Common;
using JinoOrder.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JinoOrder.Tests.Services;

/// <summary>
/// DeepLinkService 테스트
/// </summary>
public class DeepLinkServiceTests
{
    private readonly Mock<NavigationService> _mockNavigationService;
    private readonly Mock<ILogger<DeepLinkService>> _mockLogger;
    private readonly IServiceProvider _serviceProvider;
    private readonly DeepLinkService _deepLinkService;

    public DeepLinkServiceTests()
    {
        _mockNavigationService = new Mock<NavigationService>();
        _mockLogger = new Mock<ILogger<DeepLinkService>>();
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();
        _deepLinkService = new DeepLinkService(_mockNavigationService.Object, _serviceProvider, _mockLogger.Object);
    }

    #region Scheme Tests

    [Fact]
    public void Scheme_ShouldBeJinoorder()
    {
        _deepLinkService.Scheme.Should().Be("jinoorder");
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("jinoorder://orders", "orders", null)]
    [InlineData("jinoorder://menu", "menu", null)]
    [InlineData("jinoorder://customers", "customers", null)]
    [InlineData("jinoorder://settings", "settings", null)]
    [InlineData("jinoorder://stats", "stats", null)]
    [InlineData("jinoorder://history", "history", null)]
    public void Parse_ValidRoutes_ShouldReturnSuccessWithRoute(string uri, string expectedRoute, string? expectedEntityId)
    {
        // Act
        var result = _deepLinkService.Parse(uri);

        // Assert
        result.Success.Should().BeTrue();
        result.Route.Should().Be(expectedRoute);
        result.EntityId.Should().Be(expectedEntityId);
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("jinoorder://orders/123", "orders", "123")]
    [InlineData("jinoorder://menu/456", "menu", "456")]
    [InlineData("jinoorder://customers/789", "customers", "789")]
    public void Parse_ValidRoutesWithEntityId_ShouldReturnSuccessWithRouteAndEntityId(string uri, string expectedRoute, string expectedEntityId)
    {
        // Act
        var result = _deepLinkService.Parse(uri);

        // Assert
        result.Success.Should().BeTrue();
        result.Route.Should().Be(expectedRoute);
        result.EntityId.Should().Be(expectedEntityId);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyString_ShouldReturnFailure()
    {
        // Act
        var result = _deepLinkService.Parse(string.Empty);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_NullString_ShouldReturnFailure()
    {
        // Act
        var result = _deepLinkService.Parse((string)null!);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_InvalidUri_ShouldReturnFailure()
    {
        // Act
        var result = _deepLinkService.Parse("not-a-valid-uri");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("잘못된 URI 형식");
    }

    [Theory]
    [InlineData("http://orders")]
    [InlineData("https://orders")]
    [InlineData("myapp://orders")]
    public void Parse_WrongScheme_ShouldReturnFailure(string uri)
    {
        // Act
        var result = _deepLinkService.Parse(uri);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("지원하지 않는 스키마");
    }

    [Theory]
    [InlineData("jinoorder://unknown")]
    [InlineData("jinoorder://invalid-route")]
    [InlineData("jinoorder://products")]
    public void Parse_UnknownRoute_ShouldReturnFailure(string uri)
    {
        // Act
        var result = _deepLinkService.Parse(uri);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("지원하지 않는 경로");
    }

    [Fact]
    public void Parse_NullUri_ShouldReturnFailure()
    {
        // Act
        var result = _deepLinkService.Parse((Uri)null!);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null");
    }

    #endregion

    #region CanHandle Tests

    [Theory]
    [InlineData("jinoorder://orders")]
    [InlineData("jinoorder://menu")]
    [InlineData("jinoorder://customers/123")]
    [InlineData("JINOORDER://orders")] // 대소문자 무시
    public void CanHandle_ValidDeepLinks_ShouldReturnTrue(string uri)
    {
        // Act
        var result = _deepLinkService.CanHandle(uri);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://orders")]
    [InlineData("myapp://orders")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-a-uri")]
    public void CanHandle_InvalidDeepLinks_ShouldReturnFalse(string? uri)
    {
        // Act
        var result = _deepLinkService.CanHandle(uri!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanHandle_ValidUri_ShouldReturnTrue()
    {
        // Arrange
        var uri = new Uri("jinoorder://orders");

        // Act
        var result = _deepLinkService.CanHandle(uri);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanHandle_NullUri_ShouldReturnFalse()
    {
        // Act
        var result = _deepLinkService.CanHandle((Uri)null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Handle Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-uri")]
    [InlineData("http://example.com")]
    public void Handle_InvalidUri_ShouldReturnFalse(string? uri)
    {
        // Act
        var result = _deepLinkService.Handle(uri!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Case Sensitivity Tests

    [Theory]
    [InlineData("JINOORDER://ORDERS")]
    [InlineData("JinoOrder://Orders")]
    [InlineData("jinoorder://ORDERS")]
    public void Parse_CaseInsensitiveScheme_ShouldSucceed(string uri)
    {
        // Act
        var result = _deepLinkService.Parse(uri);

        // Assert
        result.Success.Should().BeTrue();
        result.Route.Should().Be("orders");
    }

    #endregion
}
