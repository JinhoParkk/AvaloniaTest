using JinoOrder.Domain.Auth;
using FluentAssertions;
using Xunit;

namespace JinoOrder.Tests.Domain;

public class AuthTokensTests
{
    [Fact]
    public void AuthTokens_ShouldStoreAccessAndRefreshTokens()
    {
        // Arrange & Act
        var tokens = new AuthTokens("access123", "refresh456");

        // Assert
        tokens.AccessToken.Should().Be("access123");
        tokens.RefreshToken.Should().Be("refresh456");
    }

    [Fact]
    public void AuthTokens_IsValid_ShouldReturnTrueWhenAccessTokenExists()
    {
        // Arrange
        var tokens = new AuthTokens("access123", "refresh456");

        // Act & Assert
        tokens.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AuthTokens_IsValid_ShouldReturnFalseWhenAccessTokenIsEmpty()
    {
        // Arrange
        var tokens = new AuthTokens("", "refresh456");

        // Act & Assert
        tokens.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AuthTokens_CanRefresh_ShouldReturnTrueWhenRefreshTokenExists()
    {
        // Arrange
        var tokens = new AuthTokens("", "refresh456");

        // Act & Assert
        tokens.CanRefresh.Should().BeTrue();
    }

    [Fact]
    public void AuthTokens_CanRefresh_ShouldReturnFalseWhenRefreshTokenIsEmpty()
    {
        // Arrange
        var tokens = new AuthTokens("access123", "");

        // Act & Assert
        tokens.CanRefresh.Should().BeFalse();
    }

    [Fact]
    public void AuthTokens_Empty_ShouldReturnEmptyTokens()
    {
        // Arrange & Act
        var tokens = AuthTokens.Empty;

        // Assert
        tokens.AccessToken.Should().BeEmpty();
        tokens.RefreshToken.Should().BeEmpty();
        tokens.IsValid.Should().BeFalse();
        tokens.CanRefresh.Should().BeFalse();
    }
}
