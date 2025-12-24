using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AvaloniaApplication1.Domain.Auth;
using AvaloniaApplication1.Infrastructure.Http;
using FluentAssertions;
using Moq;
using Xunit;

namespace AvaloniaApplication1.Tests.Infrastructure;

public class ApiClientTests
{
    private readonly Mock<ITokenStorage> _tokenStorageMock;
    private readonly Mock<ITokenRefreshService> _tokenRefreshServiceMock;

    public ApiClientTests()
    {
        _tokenStorageMock = new Mock<ITokenStorage>();
        _tokenRefreshServiceMock = new Mock<ITokenRefreshService>();
    }

    [Fact]
    public async Task GetAsync_WhenSuccess_ShouldReturnData()
    {
        // Arrange
        var expectedData = new TestDto { Id = 1, Name = "Test" };
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedData));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        _tokenStorageMock.Setup(x => x.GetTokens()).Returns(new AuthTokens("validToken", "refresh"));

        var apiClient = new ApiClient(httpClient, _tokenStorageMock.Object, _tokenRefreshServiceMock.Object);

        // Act
        var result = await apiClient.GetAsync<TestDto>("/test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_When401_ShouldAttemptRefreshAndRetry()
    {
        // Arrange
        var expectedData = new TestDto { Id = 1, Name = "Test" };
        var callCount = 0;
        var handler = new MockHttpMessageHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedData)
            };
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        _tokenStorageMock.Setup(x => x.GetTokens())
            .Returns(new AuthTokens("oldToken", "refreshToken"));

        _tokenRefreshServiceMock.Setup(x => x.RefreshAsync("refreshToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync("newAccessToken");

        var apiClient = new ApiClient(httpClient, _tokenStorageMock.Object, _tokenRefreshServiceMock.Object);

        // Act
        var result = await apiClient.GetAsync<TestDto>("/test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        callCount.Should().Be(2); // 첫 번째 401, 두 번째 성공
        _tokenStorageMock.Verify(x => x.UpdateAccessToken("newAccessToken"), Times.Once);
    }

    [Fact]
    public async Task GetAsync_When401AndRefreshFails_ShouldClearTokensAndReturnUnauthorized()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        _tokenStorageMock.Setup(x => x.GetTokens())
            .Returns(new AuthTokens("oldToken", "refreshToken"));

        _tokenRefreshServiceMock.Setup(x => x.RefreshAsync("refreshToken", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null); // 리프레시 실패

        var apiClient = new ApiClient(httpClient, _tokenStorageMock.Object, _tokenRefreshServiceMock.Object);

        // Act
        var result = await apiClient.GetAsync<TestDto>("/test");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.IsUnauthorized.Should().BeTrue();
        _tokenStorageMock.Verify(x => x.ClearTokens(), Times.Once);
    }

    [Fact]
    public async Task PostAsync_ShouldSendDataAndReturnResponse()
    {
        // Arrange
        var requestData = new TestRequest { Value = "test" };
        var expectedResponse = new TestDto { Id = 1, Name = "Created" };
        var handler = new MockHttpMessageHandler(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(expectedResponse));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        _tokenStorageMock.Setup(x => x.GetTokens()).Returns(new AuthTokens("validToken", "refresh"));

        var apiClient = new ApiClient(httpClient, _tokenStorageMock.Object, _tokenRefreshServiceMock.Object);

        // Act
        var result = await apiClient.PostAsync<TestRequest, TestDto>("/test", requestData);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Created");
    }

    [Fact]
    public async Task PostWithoutAuthAsync_ShouldNotIncludeAuthorizationHeader()
    {
        // Arrange
        var requestData = new TestRequest { Value = "test" };
        var expectedResponse = new TestDto { Id = 1, Name = "Created" };
        HttpRequestMessage? capturedRequest = null;

        var handler = new MockHttpMessageHandler(req =>
        {
            capturedRequest = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedResponse)
            };
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com") };

        _tokenStorageMock.Setup(x => x.GetTokens()).Returns(new AuthTokens("validToken", "refresh"));

        var apiClient = new ApiClient(httpClient, _tokenStorageMock.Object, _tokenRefreshServiceMock.Object);

        // Act
        var result = await apiClient.PostWithoutAuthAsync<TestRequest, TestDto>("/test", requestData);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().BeNull();
    }

    // Test DTOs
    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    // Mock HTTP Handler
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string? content = null)
        {
            _handler = _ => new HttpResponseMessage(statusCode)
            {
                Content = content != null ? new StringContent(content, System.Text.Encoding.UTF8, "application/json") : null
            };
        }

        public MockHttpMessageHandler(Func<HttpResponseMessage> responseFactory)
        {
            _handler = _ => responseFactory();
        }

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
