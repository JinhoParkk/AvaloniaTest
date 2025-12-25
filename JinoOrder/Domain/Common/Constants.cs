using System;

namespace JinoOrder.Domain.Common;

/// <summary>
/// 검증 메시지 상수
/// </summary>
public static class ValidationMessages
{
    public const string UsernameRequired = "아이디를 입력해주세요.";
    public const string PasswordRequired = "비밀번호를 입력해주세요.";
    public const string InvalidCredentials = "아이디 또는 비밀번호가 올바르지 않습니다.";
    public const string LoginError = "로그인 중 오류가 발생했습니다. 다시 시도해주세요.";
    public const string NetworkError = "네트워크 오류가 발생했습니다. 인터넷 연결을 확인해주세요.";
    public const string TimeoutError = "요청 시간이 초과되었습니다. 다시 시도해주세요.";
    public const string ServerError = "서버 오류가 발생했습니다. 잠시 후 다시 시도해주세요.";
    public const string DataLoadError = "데이터를 불러오지 못했습니다.";
    public const string SaveError = "저장 중 오류가 발생했습니다.";
    public const string DeleteError = "삭제 중 오류가 발생했습니다.";
    public const string InvalidInput = "입력값이 올바르지 않습니다.";
    public const string OperationCancelled = "작업이 취소되었습니다.";

    // 설정 관련
    public const string InvalidTimeRange = "영업 시작 시간은 종료 시간보다 빨라야 합니다.";
    public const string InvalidPickupTime = "픽업 시간은 1분 이상이어야 합니다.";
    public const string MinPickupTimeTooLarge = "최소 픽업 시간은 최대 픽업 시간보다 작아야 합니다.";
}

/// <summary>
/// 타이밍 관련 상수
/// </summary>
public static class TimingConstants
{
    public const int SaveMessageDisplayMs = 2000;
    public const int NewOrderTimerIntervalMs = 30000;
    public const int LocationTimeoutSeconds = 10;
    public const int ApiTimeoutSeconds = 30;
    public const int PopularItemsDaysBack = 7;
    public const int PopularItemsCount = 5;
    public const int TokenRefreshBufferMinutes = 5;
}

/// <summary>
/// 파일 관련 상수
/// </summary>
public static class FileConstants
{
    public const string PreferencesFileName = "preferences.json";
    public const string SettingsFileName = "settings.json";
    public const string AppFolderName = "JinoOrder";
    public const string LogsFolderName = "logs";
}

/// <summary>
/// 앱 설정 상수
/// </summary>
public static class AppConstants
{
    public const string DefaultStoreName = "지노커피";
    public const string DeepLinkScheme = "jinoorder";
    public const string AuthorizationScheme = "Bearer";
    public const int DefaultMinPickupTime = 10;
    public const int DefaultMaxPickupTime = 20;
    public static readonly TimeSpan DefaultOpenTime = new(9, 0, 0);
    public static readonly TimeSpan DefaultCloseTime = new(22, 0, 0);
}

/// <summary>
/// 라우트 상수
/// </summary>
public static class Routes
{
    public const string Orders = "orders";
    public const string History = "history";
    public const string Menu = "menu";
    public const string Customers = "customers";
    public const string Statistics = "stats";
    public const string Settings = "settings";

    public static readonly string[] AllRoutes =
    {
        Orders, History, Menu, Customers, Statistics, Settings
    };

    public static bool IsValidRoute(string route)
    {
        return Array.IndexOf(AllRoutes, route?.ToLowerInvariant()) >= 0;
    }
}

/// <summary>
/// API 관련 상수
/// </summary>
public static class ApiConstants
{
    public const string DefaultBaseUrl = "https://api.passorder.com";
    public const string RefreshTokenEndpoint = "/auth/refresh";
    public const string LoginEndpoint = "/auth/login";
    public const string LogoutEndpoint = "/auth/logout";
}

/// <summary>
/// 주문 취소 사유 상수
/// </summary>
public static class CancellationReasons
{
    public const string StoreReason = "가게 사정으로 취소";
    public const string CustomerRequest = "고객 요청";
    public const string OutOfStock = "재료 소진";
    public const string SystemError = "시스템 오류";
}
