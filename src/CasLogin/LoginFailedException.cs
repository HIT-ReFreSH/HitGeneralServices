using System;

namespace HitRefresh.HitGeneralServices.CasLogin;

/// <summary>
///     CAS登陆失败
/// </summary>
public class LoginFailedException : Exception
{
    /// <summary>
    ///     创建登陆失败异常实例
    /// </summary>
    /// <param name="username">登陆的账号</param>
    /// <param name="message">CAS服务端返回的失败原因</param>
    public LoginFailedException(string username, string message) : base(message)
    {
        Username = username;
    }

    /// <summary>
    ///     登陆的账号
    /// </summary>
    public string Username { get; }
}