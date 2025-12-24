using AvaloniaApplication1.UseCases.Services.Auth;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Infrastructure.Http.Refit;
using FluentAssertions;
using Moq;
using Refit;
using Xunit;

namespace AvaloniaApplication1.Tests.Infrastructure;

public class RefitAuthApiServiceTests
{
    private readonly Mock<IAuthApi> _authApiMock;
    private readonly Mock<ITokenStorage> _tokenStorageMock;
    private readonly RefitAuthApiService _service;

    public RefitAuthApiServiceTests()
    {
        _authApiMock = new Mock<IAuthApi>();
        _tokenStorageMock = new Mock<ITokenStorage>();
        _service = new RefitAuthApiService(_authApiMock.Object, _tokenStorageMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccess_ShouldSaveTokensAndReturnResponse()
    {
        // Arrange
        var loginResponse = new LoginResponse
        {
            AccessToken = "access123",
            RefreshToken = "refresh456",
            Username = "testuser"
        };

        _authApiMock.Setup(x => x.LoginAsync(
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _service.LoginAsync("testuser", "password123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access123");
        result.Data.RefreshToken.Should().Be("refresh456");

        _tokenStorageMock.Verify(x => x.SaveTokens(It.Is<AuthTokens>(t =>
            t.AccessToken == "access123" && t.RefreshToken == "refresh456")), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenFailed_ShouldNotSaveTokens()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Post,
            response,
            new RefitSettings());

        _authApiMock.Setup(x => x.LoginAsync(
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        // Act
        var result = await _service.LoginAsync("testuser", "wrongpassword");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(401);

        _tokenStorageMock.Verify(x => x.SaveTokens(It.IsAny<AuthTokens>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenSuccess_ShouldUpdateAccessToken()
    {
        // Arrange
        var refreshResponse = new RefreshTokenResponse
        {
            AccessToken = "newAccess123"
        };

        _authApiMock.Setup(x => x.RefreshTokenAsync(
                It.IsAny<RefreshTokenRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshResponse);

        // Act
        var result = await _service.RefreshTokenAsync("oldRefreshToken");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("newAccess123");

        _tokenStorageMock.Verify(x => x.UpdateAccessToken("newAccess123"), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenNewRefreshTokenProvided_ShouldSaveBothTokens()
    {
        // Arrange
        var refreshResponse = new RefreshTokenResponse
        {
            AccessToken = "newAccess123",
            RefreshToken = "newRefresh456"
        };

        _authApiMock.Setup(x => x.RefreshTokenAsync(
                It.IsAny<RefreshTokenRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshResponse);

        // Act
        var result = await _service.RefreshTokenAsync("oldRefreshToken");

        // Assert
        result.IsSuccess.Should().BeTrue();

        _tokenStorageMock.Verify(x => x.SaveTokens(It.Is<AuthTokens>(t =>
            t.AccessToken == "newAccess123" && t.RefreshToken == "newRefresh456")), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearTokens()
    {
        // Arrange
        _authApiMock.Setup(x => x.LogoutAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LogoutAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tokenStorageMock.Verify(x => x.ClearTokens(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenApiFails_ShouldStillClearTokens()
    {
        // Arrange
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Post,
            response,
            new RefitSettings());

        _authApiMock.Setup(x => x.LogoutAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        // Act
        var result = await _service.LogoutAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tokenStorageMock.Verify(x => x.ClearTokens(), Times.Once);
    }
}
