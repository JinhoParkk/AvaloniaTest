using System;
using System.Collections.Generic;
using System.Linq;
using JinoOrder.Application.Common;

namespace JinoOrder.Desktop.Configuration;

/// <summary>
/// QA 시뮬레이션용 커맨드라인 파서
/// </summary>
public static class CommandLineParser
{
    public static QASimulationOptions Parse(string[] args)
    {
        if (args.Length == 0)
            return QASimulationOptions.None;

        // 도움말 체크
        if (args.Contains("--qa-help") || args.Contains("-qh"))
        {
            PrintQAHelp();
            Environment.Exit(0);
        }

        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--") || arg.StartsWith("-"))
            {
                var key = arg.TrimStart('-').ToLowerInvariant();
                var value = (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    ? args[++i]
                    : "true";

                // 키 이름 정규화
                key = key switch
                {
                    "p" or "platform" or "simulate-platform" => "platform",
                    "o" or "os" or "simulate-os" => "os",
                    "r" or "res" or "resolution" => "resolution",
                    "d" or "device" => "device",
                    _ => key
                };

                options[key] = value;
            }
        }

        // 디바이스 프리셋 우선 처리
        if (options.TryGetValue("device", out var deviceName))
        {
            if (QASimulationOptions.DevicePresets.TryGetValue(deviceName, out var preset))
            {
                return new QASimulationOptions
                {
                    IsEnabled = true,
                    SimulatedPlatform = options.TryGetValue("platform", out var p)
                        ? Enum.Parse<PlatformType>(p, true)
                        : preset.Platform,
                    SimulatedOS = options.TryGetValue("os", out var o)
                        ? Enum.Parse<OSType>(o, true)
                        : preset.OS,
                    SimulatedResolution = options.TryGetValue("resolution", out var r)
                        ? ScreenResolution.Parse(r)
                        : preset.Resolution,
                    DevicePreset = deviceName
                };
            }
            else
            {
                Console.Error.WriteLine($"알 수 없는 디바이스 프리셋: {deviceName}");
                Console.Error.WriteLine($"사용 가능한 프리셋: {string.Join(", ", QASimulationOptions.DevicePresets.Keys)}");
                Environment.Exit(1);
            }
        }

        // 시뮬레이션 옵션 확인
        bool hasSimulationOption = options.ContainsKey("platform") ||
                                   options.ContainsKey("os") ||
                                   options.ContainsKey("resolution");

        if (!hasSimulationOption)
            return QASimulationOptions.None;

        return new QASimulationOptions
        {
            IsEnabled = true,
            SimulatedPlatform = options.TryGetValue("platform", out var platform)
                ? Enum.Parse<PlatformType>(platform, true)
                : null,
            SimulatedOS = options.TryGetValue("os", out var os)
                ? Enum.Parse<OSType>(os, true)
                : null,
            SimulatedResolution = options.TryGetValue("resolution", out var resolution)
                ? ScreenResolution.Parse(resolution)
                : null
        };
    }

    private static void PrintQAHelp()
    {
        Console.WriteLine(@"
JinoOrder QA 시뮬레이션 모드
============================

다양한 플랫폼 환경을 시뮬레이션하여 UI 레이아웃을 테스트합니다.

사용법:
  JinoOrder.Desktop [옵션]

옵션:
  --platform, -p <type>    플랫폼 유형: Desktop, Mobile, Web
  --os, -o <os>            OS 유형: Windows, MacOS, Linux, iOS, Android
  --resolution, -r <WxH>   화면 해상도 (예: 390x844)
  --device, -d <preset>    디바이스 프리셋 사용
  --qa-help, -qh           이 도움말 표시

디바이스 프리셋:
  iPhone14         Mobile, iOS, 390x844
  iPhone14ProMax   Mobile, iOS, 430x932
  iPadPro12        Mobile, iOS, 1024x1366
  GalaxyS23        Mobile, Android, 360x780
  Pixel7           Mobile, Android, 412x915
  Desktop1080p     Desktop, Windows, 1920x1080
  Desktop1440p     Desktop, Windows, 2560x1440
  Desktop4K        Desktop, Windows, 3840x2160

예시:
  # iPhone 14 환경 시뮬레이션
  JinoOrder.Desktop --device iPhone14

  # 커스텀 해상도로 Android 폰 시뮬레이션
  JinoOrder.Desktop -p Mobile -o Android -r 400x800

  # 가로 모드 iPad 시뮬레이션
  JinoOrder.Desktop -p Mobile -o iOS -r 1366x1024
");
    }
}
