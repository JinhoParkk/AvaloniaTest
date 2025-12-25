using System.Threading.Tasks;
using JinoOrder.Application.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace JinoOrder.Tests.Services;

/// <summary>
/// IDialogService 인터페이스 계약 테스트
/// </summary>
public class DialogServiceTests
{
    [Fact]
    public async Task ShowConfirmationAsync_WhenConfirmed_ReturnsTrue()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockDialogService.Object.ShowConfirmationAsync("Title", "Message");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShowConfirmationAsync_WhenCancelled_ReturnsFalse()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await mockDialogService.Object.ShowConfirmationAsync("Title", "Message");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShowInformationAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowInformationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var act = async () => await mockDialogService.Object.ShowInformationAsync("Title", "Message");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShowErrorAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowErrorAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var act = async () => await mockDialogService.Object.ShowErrorAsync("Error Title", "Error Message");
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("user input")]
    [InlineData("")]
    [InlineData("한글 입력 테스트")]
    public async Task ShowInputAsync_WhenUserEntersValue_ReturnsValue(string inputValue)
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowInputAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(inputValue);

        // Act
        var result = await mockDialogService.Object.ShowInputAsync("Title", "Enter value");

        // Assert
        result.Should().Be(inputValue);
    }

    [Fact]
    public async Task ShowInputAsync_WhenCancelled_ReturnsNull()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowInputAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await mockDialogService.Object.ShowInputAsync("Title", "Enter value");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ShowInputAsync_WithDefaultValue_ShouldPassDefaultValue()
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService
            .Setup(d => d.ShowInputAsync("Title", "Message", "default"))
            .ReturnsAsync("default");

        // Act
        var result = await mockDialogService.Object.ShowInputAsync("Title", "Message", "default");

        // Assert
        result.Should().Be("default");
        mockDialogService.Verify(
            d => d.ShowInputAsync("Title", "Message", "default"),
            Times.Once);
    }

    [Theory]
    [InlineData("", "message")]
    [InlineData("title", "")]
    [InlineData("긴 제목", "긴 메시지 내용입니다. 여러 줄에 걸쳐 표시될 수 있습니다.")]
    public async Task AllDialogMethods_WithVariousInputs_ShouldNotThrow(string title, string message)
    {
        // Arrange
        var mockDialogService = new Mock<IDialogService>();
        mockDialogService.Setup(d => d.ShowConfirmationAsync(title, message)).ReturnsAsync(true);
        mockDialogService.Setup(d => d.ShowInformationAsync(title, message)).Returns(Task.CompletedTask);
        mockDialogService.Setup(d => d.ShowErrorAsync(title, message)).Returns(Task.CompletedTask);
        mockDialogService.Setup(d => d.ShowInputAsync(title, message, "")).ReturnsAsync("input");

        var dialogService = mockDialogService.Object;

        // Act & Assert
        await dialogService.ShowConfirmationAsync(title, message);
        await dialogService.ShowInformationAsync(title, message);
        await dialogService.ShowErrorAsync(title, message);
        await dialogService.ShowInputAsync(title, message);

        mockDialogService.Verify(d => d.ShowConfirmationAsync(title, message), Times.Once);
        mockDialogService.Verify(d => d.ShowInformationAsync(title, message), Times.Once);
        mockDialogService.Verify(d => d.ShowErrorAsync(title, message), Times.Once);
        mockDialogService.Verify(d => d.ShowInputAsync(title, message, ""), Times.Once);
    }
}
