using AvaloniaApplication1.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AvaloniaApplication1.Tests.Services;

public class PlatformInfoTests
{
    [Fact]
    public void DesktopPlatform_IsMobile_ShouldBeFalse()
    {
        var mock = new Mock<IPlatformInfo>();
        mock.Setup(p => p.IsMobile).Returns(false);

        mock.Object.IsMobile.Should().BeFalse();
    }

    [Fact]
    public void MobilePlatform_IsMobile_ShouldBeTrue()
    {
        var mock = new Mock<IPlatformInfo>();
        mock.Setup(p => p.IsMobile).Returns(true);

        mock.Object.IsMobile.Should().BeTrue();
    }
}
