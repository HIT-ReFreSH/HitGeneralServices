using HtmlAgilityPack;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HitRefresh.HitGeneralServices.CasLogin
{
    /// <summary>
    /// 可以CAS登录的HttpClient
    /// </summary>
    public partial class LoginHttpClient : HttpClient
    {
        /// <summary>
        /// 使用Windows默认图片查看器查看验证码图片
        /// </summary>
        /// <param name="s">验证码的源头流</param>
        /// <returns>验证码结果</returns>
        public static async Task<string> Win32CaptchaInput(Stream s)
        {
            var fn = Path.GetTempFileName() + ".jpg";
            await using var fs = File.OpenWrite(fn);
            await s.CopyToAsync(fs);
            fs.Close();
            var p = new Process()
            {
                StartInfo = new()
                {
                    FileName = fn,
                    UseShellExecute = true
                }
            };
            p.Start();
            Console.WriteLine("输入验证码：");
            return Console.ReadLine() ?? "";
        }
        /// <summary>
        /// 生成使用指定查看器查看二维码的适配器
        /// </summary>
        /// <param name="pathToJpegViewer">Jpeg查看器的路径</param>
        /// <returns>使用指定查看器查看二维码的适配器</returns>
        public static Func< Stream,Task<string>> CaptchaInputFactory(string pathToJpegViewer)
        {
            return async (s) =>
            {
                var fn = Path.GetTempFileName() + ".jpg";
                await using var fs = File.OpenWrite(fn);
                await s.CopyToAsync(fs);
                fs.Close();
                var p = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = pathToJpegViewer,
                        Arguments = fn,
                        UseShellExecute = true
                    }
                };
                p.Start();
                Console.WriteLine("输入验证码：");
                return Console.ReadLine() ?? "";
            };
        }
        /// <summary>
        /// 判定当前是否已经进行登录
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAuthorized()
        {
            var rep = await GetAsync("http://ids.hit.edu.cn/authserver/login");
            return rep.RequestMessage?.RequestUri?.ToString() == LoginEndpoint;
        }
        private const string WrongPasswordOrId = "您提供的用户名或者密码有误";
        private const string LoginInfoNodePath = "/html/body/div[2]/div[2]/div[2]/div/div[3]/div/form/div";
        private const string ErrorMessagePath = "/html/body/div[2]/div[2]/div[2]/div/div[3]/div/form/span";
        private const string CaptchaUrl = "http://ids.hit.edu.cn/authserver/captcha.html";
        private const string NeedCaptchaUrl = "http://ids.hit.edu.cn/authserver/needCaptcha.html";
        private const string LoginUrl = "http://ids.hit.edu.cn/authserver/login";
        private const string LoginEndpoint = "http://ids.hit.edu.cn/authserver/index.do";
        /// <summary>
        /// 使用指定salt对明文密码实施AES加密(对应encrypt.js的encryptAES函数)
        /// </summary>
        /// <param name="message">需要加密的信息</param>
        /// <param name="salt">Salt</param>
        /// <returns></returns>
        private static string Encrypt(string message, string salt)
        {
            using var cryptoJs = new CryptoJsAes(salt);
            return cryptoJs.Encrypt(message);
        }
        /// <inheritdoc/>
        public LoginHttpClient(HttpClientHandler handler) : base(handler)
        {

        }
        /// <inheritdoc/>
        public LoginHttpClient(HttpClientHandler handler,bool disposeHandler) : base(handler,disposeHandler)
        {

        }
        /// <summary>
        /// 初始化CAS登录客户端
        /// </summary>
        public LoginHttpClient()
        {
            MaxResponseContentBufferSize = 256000;
            DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");
        }
        /// <summary>
        /// 持续尝试登录，直至满足最大尝试次数，或者抛出不能处理的异常
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="maxTrial">最大尝试次数</param>
        /// <param name="captchaGenerator">用于生成填写二维码的适配器</param>
        /// <returns></returns>
        /// <exception cref="CaptchaRequiredException">需要填写二维码，但是未提供对应的适配器</exception>
        /// <exception cref="LoginFailedException">登陆认证失败</exception>
        public async Task TryLoginFor(string username, string password, uint maxTrial, Func<Stream, Task<string>>? captchaGenerator = null)
        {
            for (var i = 0u; i < maxTrial; i++)
            {
                try
                {
                    await LoginAsync(username,password,captchaGenerator);
                }
                catch (CaptchaRequiredException)
                {
                    throw;
                }
                catch (LoginFailedException loginFailed)
                {
                    if (loginFailed.Message.Contains(WrongPasswordOrId)) throw;
                    continue;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
        /// <summary>
        /// 进行CAS登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captchaGenerator">用于生成填写二维码的适配器</param>
        /// <returns></returns>
        /// <exception cref="CaptchaRequiredException">需要填写二维码，但是未提供对应的适配器</exception>
        /// <exception cref="LoginFailedException">登陆认证失败</exception>
        public async Task LoginAsync(string username, string password,Func<Stream, Task<string>>? captchaGenerator = null)
        {
            var htmlDoc = new HtmlDocument();

            htmlDoc.Load(await GetStreamAsync(LoginUrl));
            var loginInfoNode =
                htmlDoc.DocumentNode.SelectSingleNode(LoginInfoNodePath);
            var pwdDefaultEncryptSalt = loginInfoNode.SelectSingleNode("//input[@id='pwdDefaultEncryptSalt']")
                .GetAttributeValue("value", "");
            string? captcha = null;
            var passwordEncrypt = Encrypt(
                CryptoJsAes.GetRandomString(64) + password, pwdDefaultEncryptSalt);
            var captchaRequired = await GetStringAsync(
                $"{NeedCaptchaUrl}?username={username}&pwdEncrypt2={pwdDefaultEncryptSalt}");
            if (captchaRequired == "true")
            {
                if (captchaGenerator is null)
                {
                    throw new CaptchaRequiredException();
                }


                var captchaStream =
                    await GetStreamAsync(
                        $"{CaptchaUrl}?ts={new Random().Next(0, 999)}");
                captcha = await captchaGenerator(captchaStream);
            }

            string GetValue(string name)
            {
                return loginInfoNode.SelectSingleNode($"//input[@name='{name}']").GetAttributeValue("value", "");
            }

            var postContent = new Dictionary<string, string?>
            {
                { "username", username },
                { "password", passwordEncrypt }
            };
            if (captcha is { })
                postContent.Add("captchaResponse", captcha);
            foreach (var key in new[]{ "lt",
                "dllt",
                "execution",
                "_eventId",
                "rmShown"})
            {
                postContent.Add(key, GetValue(key));
            }

            var loginResponse = await PostAsync(LoginUrl, new FormUrlEncodedContent(
                postContent.Select(p => new KeyValuePair<string?, string?>(p.Key, p.Value)
                )));

            if (loginResponse.RequestMessage?.RequestUri?.ToString() != LoginEndpoint)
            {
                htmlDoc = new();
                htmlDoc.Load(await loginResponse.Content.ReadAsStreamAsync());
                throw new LoginFailedException(username,
                    htmlDoc.DocumentNode.SelectSingleNode(ErrorMessagePath).GetDirectInnerText());

            }

        }
    }
}
