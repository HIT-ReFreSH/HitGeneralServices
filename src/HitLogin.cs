using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.CasLogin;
using Microsoft.Extensions.Configuration;

namespace HitRefresh.HitGeneralServices
{
    /// <summary>
    ///     用于Bot登录的服务
    /// </summary>
    public class HitLogin
    {
        private readonly string _password;

        private readonly string _username;

        /// <summary>
        ///     使用包含username和password字段的文件初始化登陆服务
        /// </summary>
        /// <param name="configuration"></param>
        public HitLogin(IConfiguration configuration)
        {
            _username = configuration["username"];
            _password = configuration["password"];
        }

        /// <summary>
        ///     完成了登陆的Http客户端
        /// </summary>
        public LoginHttpClient HttpClient { get; } = new();

        /// <summary>
        ///     登录
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> LoginAsync()
        {
            var report = new List<string>();
            try
            {
                await HttpClient.TryLoginFor(_username, _password, 5);
            }
            catch (CaptchaRequiredException)
            {
                Console.WriteLine("登录失败：需要验证码");
                report.Add("登录失败：需要验证码");
            }
            catch (LoginFailedException loginFailed)
            {
                Console.WriteLine($"登录失败：{loginFailed.Message}");
                report.Add($"登录失败：{loginFailed.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"登录失败：{ex.Message}");
                report.Add($"登录失败：{ex.Message}");
            }

            return report;
        }
    }
}