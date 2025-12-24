using System;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services;

public class AuthenticationService
{
    // 테스트용 하드코딩된 계정 (나중에 API로 변경 예정)
    private const string TestUsername = "admin";
    private const string TestPassword = "1234";

    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public bool Login(string username, string password)
    {
        // 로컬 인증 (테스트용)
        if (username == TestUsername && password == TestPassword)
        {
            CurrentUser = new User
            {
                Username = username,
                Token = Guid.NewGuid().ToString() // 임시 토큰 생성
            };
            return true;
        }

        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }
}
