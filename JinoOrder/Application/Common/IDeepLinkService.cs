using System;

namespace JinoOrder.Application.Common;

/// <summary>
/// 딥링크 처리 결과
/// </summary>
public record DeepLinkResult(
    bool Success,
    string? Route = null,
    string? EntityId = null,
    string? ErrorMessage = null);

/// <summary>
/// 딥링크 처리 서비스 인터페이스
/// URI 스키마: jinoorder://
///
/// 지원 경로:
/// - jinoorder://orders - 주문 목록
/// - jinoorder://orders/{orderId} - 특정 주문 상세
/// - jinoorder://menu - 메뉴 관리
/// - jinoorder://menu/{menuItemId} - 특정 메뉴 아이템
/// - jinoorder://customers - 고객 목록
/// - jinoorder://customers/{customerId} - 특정 고객 상세
/// - jinoorder://settings - 설정
/// </summary>
public interface IDeepLinkService
{
    /// <summary>
    /// URI 스키마 (jinoorder)
    /// </summary>
    string Scheme { get; }

    /// <summary>
    /// 딥링크 URI 파싱
    /// </summary>
    /// <param name="uri">딥링크 URI (예: jinoorder://orders/123)</param>
    /// <returns>파싱 결과</returns>
    DeepLinkResult Parse(Uri uri);

    /// <summary>
    /// 딥링크 URI 문자열 파싱
    /// </summary>
    /// <param name="uriString">딥링크 URI 문자열</param>
    /// <returns>파싱 결과</returns>
    DeepLinkResult Parse(string uriString);

    /// <summary>
    /// 딥링크 처리 (네비게이션 수행)
    /// </summary>
    /// <param name="uri">딥링크 URI</param>
    /// <returns>처리 성공 여부</returns>
    bool Handle(Uri uri);

    /// <summary>
    /// 딥링크 처리 (네비게이션 수행)
    /// </summary>
    /// <param name="uriString">딥링크 URI 문자열</param>
    /// <returns>처리 성공 여부</returns>
    bool Handle(string uriString);

    /// <summary>
    /// 해당 URI가 이 앱에서 처리 가능한 딥링크인지 확인
    /// </summary>
    /// <param name="uri">확인할 URI</param>
    /// <returns>처리 가능 여부</returns>
    bool CanHandle(Uri uri);

    /// <summary>
    /// 해당 URI 문자열이 이 앱에서 처리 가능한 딥링크인지 확인
    /// </summary>
    /// <param name="uriString">확인할 URI 문자열</param>
    /// <returns>처리 가능 여부</returns>
    bool CanHandle(string uriString);
}
