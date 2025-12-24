using System;

namespace AvaloniaApplication1.Models;

/// <summary>
/// 매장 정보 모델
/// </summary>
public class Store
{
    /// <summary>
    /// 매장 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 매장명
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 매장 오픈 상태
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// 일시정지 상태 (IsOpen이 false일 때만 유효)
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// 일시정지 종료 일시 (IsPaused가 true일 때)
    /// </summary>
    public DateTime? PausedUntil { get; set; }

    /// <summary>
    /// 최소 수령 시간 (분)
    /// </summary>
    public int MinPickupTime { get; set; }

    /// <summary>
    /// 최대 수령 시간 (분) - 특정 매장만
    /// </summary>
    public int? MaxPickupTime { get; set; }

    /// <summary>
    /// 현재 매장 상태 텍스트
    /// </summary>
    public string StatusText
    {
        get
        {
            if (IsOpen) return "영업중";
            if (IsPaused && PausedUntil.HasValue)
                return $"일시정지 ({PausedUntil:HH:mm}까지)";
            if (IsPaused)
                return "일시정지";
            return "영업종료";
        }
    }

    /// <summary>
    /// 수령 시간 텍스트
    /// </summary>
    public string PickupTimeText
    {
        get
        {
            if (MaxPickupTime.HasValue)
                return $"{MinPickupTime}~{MaxPickupTime}분";
            return $"{MinPickupTime}분";
        }
    }
}
