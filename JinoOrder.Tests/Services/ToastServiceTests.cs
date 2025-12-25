using System.Threading.Tasks;
using JinoOrder.Application.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace JinoOrder.Tests.Services;

/// <summary>
/// IToastService 인터페이스 계약 테스트
/// </summary>
public class ToastServiceTests
{
    [Fact]
    public async Task ShowAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockToastService = new Mock<IToastService>();
        mockToastService
            .Setup(t => t.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ToastType>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockToastService.Object.ShowAsync("Title", "Message");

        // Assert
        mockToastService.Verify(
            t => t.ShowAsync("Title", "Message", ToastType.Information, 3000),
            Times.Once);
    }

    [Theory]
    [InlineData("", "message")]
    [InlineData("title", "")]
    [InlineData("title", "message")]
    [InlineData("긴 제목 테스트", "긴 메시지 테스트입니다")]
    public async Task ShowAsync_WithVariousInputs_ShouldNotThrow(string title, string message)
    {
        // Arrange
        var mockToastService = new Mock<IToastService>();
        mockToastService
            .Setup(t => t.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ToastType>(),
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var act = async () => await mockToastService.Object.ShowAsync(title, message);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(ToastType.Information)]
    [InlineData(ToastType.Success)]
    [InlineData(ToastType.Warning)]
    [InlineData(ToastType.Error)]
    public async Task ShowAsync_WithDifferentTypes_ShouldWork(ToastType type)
    {
        // Arrange
        var mockToastService = new Mock<IToastService>();
        mockToastService
            .Setup(t => t.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                type,
                It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockToastService.Object.ShowAsync("Title", "Message", type);

        // Assert
        mockToastService.Verify(
            t => t.ShowAsync("Title", "Message", type, 3000),
            Times.Once);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(3000)]
    [InlineData(5000)]
    public async Task ShowAsync_WithCustomDuration_ShouldWork(int durationMs)
    {
        // Arrange
        var mockToastService = new Mock<IToastService>();
        mockToastService
            .Setup(t => t.ShowAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ToastType>(),
                durationMs))
            .Returns(Task.CompletedTask);

        // Act
        await mockToastService.Object.ShowAsync("Title", "Message", ToastType.Information, durationMs);

        // Assert
        mockToastService.Verify(
            t => t.ShowAsync("Title", "Message", ToastType.Information, durationMs),
            Times.Once);
    }
}
