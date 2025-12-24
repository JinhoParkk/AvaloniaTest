using System;
using System.IO;
using System.Text.Json;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services;

public class PreferencesService
{
    private readonly string _preferencesPath;
    private const string FileName = "preferences.json";

    public PreferencesService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "AvaloniaApplication1");

        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _preferencesPath = Path.Combine(appFolder, FileName);
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
}
