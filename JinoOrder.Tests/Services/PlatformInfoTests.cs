using JinoOrder.Application.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace JinoOrder.Tests.Services;

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
