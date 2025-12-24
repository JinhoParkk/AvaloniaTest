using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Infrastructure.Http;
using FluentAssertions;
using Xunit;

namespace AvaloniaApplication1.Tests.Infrastructure;

public class TokenStorageTests
{
    [Fact]
    public void GetTokens_WhenNoTokensSaved_ShouldReturnEmptyTokens()
    {
        // Arrange
        var storage = new InMemoryTokenStorage();

        // Act
        var tokens = storage.GetTokens();

        // Assert
        tokens.IsValid.Should().BeFalse();
        tokens.CanRefresh.Should().BeFalse();
    }

    [Fact]
    public void SaveTokens_ShouldStoreTokens()
    {
        // Arrange
        var storage = new InMemoryTokenStorage();
        var tokens = new AuthTokens("access123", "refresh456");

        // Act
        storage.SaveTokens(tokens);
        var result = storage.GetTokens();

        // Assert
        result.AccessToken.Should().Be("access123");
        result.RefreshToken.Should().Be("refresh456");
    }

    [Fact]
    public void ClearTokens_ShouldRemoveAllTokens()
    {
        // Arrange
        var storage = new InMemoryTokenStorage();
        storage.SaveTokens(new AuthTokens("access123", "refresh456"));

        // Act
        storage.ClearTokens();
        var result = storage.GetTokens();

        // Assert
        result.IsValid.Should().BeFalse();
        result.CanRefresh.Should().BeFalse();
    }

    [Fact]
    public void UpdateAccessToken_ShouldOnlyUpdateAccessToken()
    {
        // Arrange
        var storage = new InMemoryTokenStorage();
        storage.SaveTokens(new AuthTokens("oldAccess", "refresh456"));

        // Act
        storage.UpdateAccessToken("newAccess");
        var result = storage.GetTokens();

        // Assert
        result.AccessToken.Should().Be("newAccess");
        result.RefreshToken.Should().Be("refresh456");
    }
}
