using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace JinoOrder.Infrastructure.Logging;

/// <summary>
/// Serilog 기반 로깅 구성
/// </summary>
public static class LoggingConfiguration
{
    private const string LogFileTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
    private const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private const string LogFileName = "jinoorder-.log";
    private const string AppFolderName = "JinoOrder";
    private const string LogsFolderName = "logs";

    /// <summary>
    /// 로그 파일 저장 경로
    /// </summary>
    public static string LogFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName,
        LogsFolderName,
        LogFileName);

    /// <summary>
    /// Serilog 로거 초기화
    /// </summary>
    public static void Initialize(bool isDevelopment = false)
    {
        var logDirectory = Path.GetDirectoryName(LogFilePath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        var configuration = new LoggerConfiguration()
            .MinimumLevel.Is(isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: LogFilePath,
                outputTemplate: LogFileTemplate,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));

        if (isDevelopment)
        {
            configuration.WriteTo.Console(outputTemplate: ConsoleTemplate);
        }

        Log.Logger = configuration.CreateLogger();
        Log.Information("JinoOrder 앱이 시작되었습니다. 로그 파일: {LogPath}", LogFilePath);
    }

    /// <summary>
    /// 로깅 종료 (앱 종료 시 호출)
    /// </summary>
    public static void Shutdown()
    {
        Log.Information("JinoOrder 앱이 종료됩니다.");
        Log.CloseAndFlush();
    }

    /// <summary>
    /// DI 컨테이너에 로깅 서비스 등록
    /// </summary>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        return services;
    }
}
