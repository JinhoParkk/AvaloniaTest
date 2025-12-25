using System;

namespace JinoOrder.Application.Common;

/// <summary>
/// 화면 해상도
/// </summary>
public record ScreenResolution(int Width, int Height)
{
    /// <summary>
    /// 세로 모드 여부
    /// </summary>
    public bool IsPortrait => Height > Width;

    /// <summary>
    /// 가로 모드 여부
    /// </summary>
    public bool IsLandscape => Width >= Height;

    /// <summary>
    /// 화면 비율
    /// </summary>
    public double AspectRatio => (double)Width / Height;

    // 디바이스 프리셋
    public static ScreenResolution iPhone14 => new(390, 844);
    public static ScreenResolution iPhone14ProMax => new(430, 932);
    public static ScreenResolution iPadPro12 => new(1024, 1366);
    public static ScreenResolution GalaxyS23 => new(360, 780);
    public static ScreenResolution Pixel7 => new(412, 915);
    public static ScreenResolution Desktop1080p => new(1920, 1080);
    public static ScreenResolution Desktop1440p => new(2560, 1440);
    public static ScreenResolution Desktop4K => new(3840, 2160);

    /// <summary>
    /// 문자열에서 해상도 파싱 (예: "1920x1080")
    /// </summary>
    public static ScreenResolution Parse(string value)
    {
        var parts = value.ToLowerInvariant().Split('x');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out var width) ||
            !int.TryParse(parts[1], out var height))
        {
            throw new ArgumentException($"잘못된 해상도 형식: {value}. 예상 형식: WIDTHxHEIGHT (예: 1920x1080)");
        }
        return new ScreenResolution(width, height);
    }

    public override string ToString() => $"{Width}x{Height}";
}
