namespace JinoOrder.Domain.Settings;

/// <summary>
/// 앱 설정 모델
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 매장명
    /// </summary>
    public string StoreName { get; set; } = "지노커피";

    /// <summary>
    /// 최소 수령 시간 (분)
    /// </summary>
    public int MinPickupTime { get; set; } = 10;

    /// <summary>
    /// 최대 수령 시간 (분)
    /// </summary>
    public int? MaxPickupTime { get; set; } = 20;

    /// <summary>
    /// 영업 시작 시간
    /// </summary>
    public TimeSpan OpenTime { get; set; } = new TimeSpan(9, 0, 0);

    /// <summary>
    /// 영업 종료 시간
    /// </summary>
    public TimeSpan CloseTime { get; set; } = new TimeSpan(22, 0, 0);

    /// <summary>
    /// 새 주문 알림 사운드 활성화
    /// </summary>
    public bool EnableOrderSound { get; set; } = true;

    /// <summary>
    /// 새 주문 알림 활성화
    /// </summary>
    public bool EnableOrderNotification { get; set; } = true;

    /// <summary>
    /// 자동 영업 시작/종료 활성화
    /// </summary>
    public bool EnableAutoOpenClose { get; set; } = false;

    /// <summary>
    /// 자동 주문 접수 활성화
    /// </summary>
    public bool EnableAutoAccept { get; set; } = false;

    /// <summary>
    /// 자동 접수 시 기본 준비 시간 (분)
    /// </summary>
    public int AutoAcceptPrepTime { get; set; } = 15;
}
