using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ApiRoutes
{
    public const string AuthRegister = "/auth/register";
    public const string Status = "/status";
    public const string AuthLogin = "/auth/login";
    public const string AuthGuest = "/auth/guest"; // 예시: 게스트 로그인
    public const string AuthRefresh = "/auth/refresh";
    public const string PlayerBootstrap = "/player/bootstrap";


    public const string MeSummary = "/me/summary";
    public const string MeProfile = "/me/profile";
    public const string MeProfileUpdate = "/me/profile";
    public const string MeChangePassword = "/me/change-password";
}
