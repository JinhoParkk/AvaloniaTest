using System;
using System.IO;
using System.Text.Json;
using JinoOrder.Domain.Common;
using JinoOrder.Domain.Settings;

namespace JinoOrder.Infrastructure.Storage;

public class PreferencesService
{
    private readonly string _appFolder;
    private readonly string _preferencesPath;
    private readonly string _settingsPath;
    private const string FileName = "preferences.json";
    private const string SettingsFileName = "settings.json";

    public PreferencesService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _appFolder = Path.Combine(appDataPath, "JinoOrder");

        if (!Directory.Exists(_appFolder))
        {
            Directory.CreateDirectory(_appFolder);
        }

        _preferencesPath = Path.Combine(_appFolder, FileName);
        _settingsPath = Path.Combine(_appFolder, SettingsFileName);
    }

    public void SaveAutoLogin(User user)
    {
        try
        {
            var data = new AutoLoginData
            {
                Username = user.Username,
                Token = user.Token
            };

            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_preferencesPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save preferences: {ex.Message}");
        }
    }

    public User? LoadAutoLogin()
    {
        try
        {
            if (!File.Exists(_preferencesPath))
            {
                return null;
            }

            var json = File.ReadAllText(_preferencesPath);
            var data = JsonSerializer.Deserialize<AutoLoginData>(json);

            if (data != null && !string.IsNullOrEmpty(data.Username))
            {
                return new User
                {
                    Username = data.Username,
                    Token = data.Token
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load preferences: {ex.Message}");
        }

        return null;
    }

    public void ClearAutoLogin()
    {
        try
        {
            if (File.Exists(_preferencesPath))
            {
                File.Delete(_preferencesPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear preferences: {ex.Message}");
        }
    }

    /// <summary>
    /// RefreshToken 저장
    /// </summary>
    public void SaveRefreshToken(string refreshToken)
    {
        try
        {
            var data = LoadAutoLoginData() ?? new AutoLoginData();
            data.RefreshToken = refreshToken;
            SaveAutoLoginData(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save refresh token: {ex.Message}");
        }
    }

    /// <summary>
    /// RefreshToken 로드
    /// </summary>
    public string? LoadRefreshToken()
    {
        try
        {
            var data = LoadAutoLoginData();
            return data?.RefreshToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load refresh token: {ex.Message}");
            return null;
        }
    }

    private AutoLoginData? LoadAutoLoginData()
    {
        if (!File.Exists(_preferencesPath))
        {
            return null;
        }

        var json = File.ReadAllText(_preferencesPath);
        return JsonSerializer.Deserialize<AutoLoginData>(json);
    }

    private void SaveAutoLoginData(AutoLoginData data)
    {
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(_preferencesPath, json);
    }

    private class AutoLoginData
    {
        public string Username { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }

    #region App Settings

    /// <summary>
    /// 앱 설정 저장
    /// </summary>
    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// 앱 설정 로드
    /// </summary>
    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load settings: {ex.Message}");
            return new AppSettings();
        }
    }

    /// <summary>
    /// 설정 파일 존재 여부
    /// </summary>
    public bool HasSettings()
    {
        return File.Exists(_settingsPath);
    }

    /// <summary>
    /// 설정 초기화
    /// </summary>
    public void ResetSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to reset settings: {ex.Message}");
        }
    }

    #endregion
}
