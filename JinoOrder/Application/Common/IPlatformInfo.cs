namespace JinoOrder.Application.Common;

/// <summary>
/// 플랫폼 정보 - 모바일/데스크탑 구분
/// </summary>
public interface IPlatformInfo
{
    /// <summary>
    /// 모바일 플랫폼 여부 (iOS, Android)
    /// </summary>
    bool IsMobile { get; }
}
