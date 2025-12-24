using AvaloniaApplication1.UseCases.Services.Auth;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Domain.Common;
using AvaloniaApplication1.Infrastructure.Http;
using FluentAssertions;
using Moq;
using Xunit;

namespace AvaloniaApplication1.Tests.Application;

public class AuthApiServiceTests
{
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly Mock<ITokenStorage> _tokenStorageMock;
    private readonly AuthApiOptions _options;
    private readonly AuthApiService _service;

    public AuthApiServiceTests()
    {
        _apiClientMock = new Mock<IApiClient>();
        _tokenStorageMock = new Mock<ITokenStorage>();
        _options = new AuthApiOptions
        {
            LoginEndpoint = "/auth/login",
            RefreshTokenEndpoint = "/auth/refresh",
            LogoutEndpoint = "/auth/logout"
        };
        _service = new AuthApiService(_apiClientMock.Object, _tokenStorageMock.Object, _options);
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

        _apiClientMock.Setup(x => x.PostWithoutAuthAsync<LoginRequest, LoginResponse>(
                _options.LoginEndpoint,
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<LoginResponse>.Success(loginResponse));

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
        _apiClientMock.Setup(x => x.PostWithoutAuthAsync<LoginRequest, LoginResponse>(
                _options.LoginEndpoint,
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<LoginResponse>.Failure("Invalid credentials", 401));

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

        _apiClientMock.Setup(x => x.PostWithoutAuthAsync<RefreshTokenRequest, RefreshTokenResponse>(
                _options.RefreshTokenEndpoint,
                It.IsAny<RefreshTokenRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<RefreshTokenResponse>.Success(refreshResponse));

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

        _apiClientMock.Setup(x => x.PostWithoutAuthAsync<RefreshTokenRequest, RefreshTokenResponse>(
                _options.RefreshTokenEndpoint,
                It.IsAny<RefreshTokenRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<RefreshTokenResponse>.Success(refreshResponse));

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
        _apiClientMock.Setup(x => x.PostAsync<object>(
                _options.LogoutEndpoint!,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult<bool>.Success(true));

        // Act
        var result = await _service.LogoutAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tokenStorageMock.Verify(x => x.ClearTokens(), Times.Once);
    }
}
