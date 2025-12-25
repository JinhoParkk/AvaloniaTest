using AvaloniaApplication1.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AvaloniaApplication1.Tests.Services;

/// <summary>
/// IPlatformInfo 인터페이스 계약 테스트
/// </summary>
public class PlatformInfoTests
{
    [Fact]
    public void DesktopPlatform_ShouldSupportWindowControls()
    {
        // Arrange
        var platformInfo = CreateDesktopPlatformInfo();

        // Assert
        platformInfo.SupportsWindowControls.Should().BeTrue();
        platformInfo.IsMobile.Should().BeFalse();
        platformInfo.IsTouchPrimary.Should().BeFalse();
        platformInfo.PlatformName.Should().Be("Desktop");
    }

    [Fact]
    public void MobilePlatform_ShouldNotSupportWindowControls()
    {
        // Arrange
        var platformInfo = CreateMobilePlatformInfo();

        // Assert
        platformInfo.SupportsWindowControls.Should().BeFalse();
        platformInfo.IsMobile.Should().BeTrue();
        platformInfo.IsTouchPrimary.Should().BeTrue();
        platformInfo.PlatformName.Should().Be("iOS");
    }

    [Theory]
    [InlineData(true, false, false, "Desktop")]
    [InlineData(false, true, true, "iOS")]
    [InlineData(false, true, true, "Android")]
    public void PlatformInfo_ShouldReturnCorrectValues(
        bool supportsWindowControls,
        bool isMobile,
        bool isTouchPrimary,
        string platformName)
    {
        // Arrange
        var mockPlatformInfo = new Mock<IPlatformInfo>();
        mockPlatformInfo.Setup(p => p.SupportsWindowControls).Returns(supportsWindowControls);
        mockPlatformInfo.Setup(p => p.IsMobile).Returns(isMobile);
        mockPlatformInfo.Setup(p => p.IsTouchPrimary).Returns(isTouchPrimary);
        mockPlatformInfo.Setup(p => p.PlatformName).Returns(platformName);

        // Act
        var platformInfo = mockPlatformInfo.Object;

        // Assert
        platformInfo.SupportsWindowControls.Should().Be(supportsWindowControls);
        platformInfo.IsMobile.Should().Be(isMobile);
        platformInfo.IsTouchPrimary.Should().Be(isTouchPrimary);
        platformInfo.PlatformName.Should().Be(platformName);
    }

    /// <summary>
    /// Desktop 플랫폼 정보 Mock 생성
    /// </summary>
    private static IPlatformInfo CreateDesktopPlatformInfo()
    {
        var mock = new Mock<IPlatformInfo>();
        mock.Setup(p => p.SupportsWindowControls).Returns(true);
        mock.Setup(p => p.IsMobile).Returns(false);
        mock.Setup(p => p.IsTouchPrimary).Returns(false);
        mock.Setup(p => p.PlatformName).Returns("Desktop");
        return mock.Object;
    }

    /// <summary>
    /// Mobile 플랫폼 정보 Mock 생성
    /// </summary>
    private static IPlatformInfo CreateMobilePlatformInfo()
    {
        var mock = new Mock<IPlatformInfo>();
        mock.Setup(p => p.SupportsWindowControls).Returns(false);
        mock.Setup(p => p.IsMobile).Returns(true);
        mock.Setup(p => p.IsTouchPrimary).Returns(true);
        mock.Setup(p => p.PlatformName).Returns("iOS");
        return mock.Object;
    }
}
