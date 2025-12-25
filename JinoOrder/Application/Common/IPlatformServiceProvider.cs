using Microsoft.Extensions.DependencyInjection;

namespace JinoOrder.Application.Common;

/// <summary>
/// 플랫폼별 서비스 등록을 위한 인터페이스
/// 리플렉션 대신 컴파일 타임에 안전한 방식으로 플랫폼 서비스 등록
/// </summary>
public interface IPlatformServiceProvider
{
    /// <summary>
    /// 플랫폼별 서비스를 IServiceCollection에 등록
    /// </summary>
    void ConfigureServices(IServiceCollection services);
}
