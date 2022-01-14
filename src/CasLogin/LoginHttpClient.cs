using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HitRefresh.HitGeneralServices.CasLogin
{
    /// <summary>
    ///     可以CAS登录的HttpClient
    /// </summary>
    public partial class LoginHttpClient : HttpClient
    {
        private const string WrongPasswordOrId = "您提供的用户名或者密码有误";
        private const string LoginInfoNodePath = "//*[@id='pwdFromId']/div";
        private const string ErrorMessagePath = "//*[@id='formErrorTip2']/span";
        private const string CaptchaUrl = "https://ids.hit.edu.cn/authserver/getCaptcha.htl";
        private const string NeedCaptchaUrl = "https://ids.hit.edu.cn/authserver/checkNeedCaptcha.htl";
        private const string LoginUrl = "https://ids.hit.edu.cn/authserver/login";
        private static readonly string[] LoginEndpoints = {
            "https://ids.hit.edu.cn/personalInfo/personCenter/index.html",
            "https://ids.hit.edu.cn/personalInfo/personalMobile/index.html"
        };

        /// <inheritdoc />
        public LoginHttpClient(HttpClientHandler handler) : base(handler)
        {

        }

        /// <inheritdoc />
        public LoginHttpClient(HttpClientHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {

        }

        /// <summary>
        ///     初始化CAS登录客户端
        /// </summary>
        public LoginHttpClient()
        {
            MaxResponseContentBufferSize = 256000;
            DefaultRequestHeaders.Add("user-agent",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");
        }

        /// <summary>
        ///     使用Windows默认图片查看器查看验证码图片
        /// </summary>
        /// <param name="s">验证码的源头流</param>
        /// <returns>验证码结果</returns>
        public static async Task<string> Win32CaptchaInput(Stream s)
        {
            var fn = Path.GetTempFileName() + ".jpg";
            await using var fs = File.OpenWrite(fn);
            await s.CopyToAsync(fs);
            fs.Close();
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
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
        ///     生成使用指定查看器查看验证码的适配器
        /// </summary>
        /// <param name="pathToJpegViewer">Jpeg查看器的路径</param>
        /// <returns>使用指定查看器查看验证码的适配器</returns>
        public static Func<Stream, Task<string>> CaptchaInputFactory(string pathToJpegViewer)
        {
            return async s =>
            {
                var fn = Path.GetTempFileName() + ".jpg";
                await using var fs = File.OpenWrite(fn);
                await s.CopyToAsync(fs);
                fs.Close();
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
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
        ///     判定当前是否已经进行登录
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAuthorized()
        {
            var rep = await GetAsync("http://ids.hit.edu.cn/authserver/login");
            return LoginEndpoints.Any(x => x == rep.RequestMessage?.RequestUri?.ToString());
        }

        /// <summary>
        ///     使用指定salt对明文密码实施AES加密(对应encrypt.js的encryptAES函数)
        /// </summary>
        /// <param name="message">需要加密的信息</param>
        /// <param name="salt">Salt</param>
        /// <returns></returns>
        private static string Encrypt(string message, string salt)
        {
            using var cryptoJs = new CryptoJsAes(salt);
            return cryptoJs.Encrypt(message);
        }

        /// <summary>
        ///     持续尝试登录，直至满足最大尝试次数，或者抛出不能处理的异常
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="maxTrial">最大尝试次数</param>
        /// <param name="captchaGenerator">用于生成填写验证码的适配器</param>
        /// <returns></returns>
        /// <exception cref="CaptchaRequiredException">需要填写验证码，但是未提供对应的适配器</exception>
        /// <exception cref="LoginFailedException">登陆认证失败</exception>
        public async Task TryLoginFor(string username, string password, uint maxTrial,
            Func<Stream, Task<string>>? captchaGenerator = null)
        {
            Exception? lastEx = null;
            for (var i = 0u; i < maxTrial; i++)
                try
                {
                    await LoginAsync(username, password, captchaGenerator);
                    if (await IsAuthorized()) return;
                }
                catch (CaptchaRequiredException)
                {
                    throw;
                }
                catch (LoginFailedException loginFailed)
                {
                    if (loginFailed.Message.Contains(WrongPasswordOrId)) throw;
                    lastEx = loginFailed;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }

            if (lastEx is not null) throw lastEx;
        }

        /// <summary>
        ///     进行CAS登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captchaGenerator">用于生成填写验证码的适配器</param>
        /// <returns></returns>
        /// <exception cref="CaptchaRequiredException">需要填写验证码，但是未提供对应的适配器</exception>
        /// <exception cref="LoginFailedException">登陆认证失败</exception>
        public async Task LoginAsync(string username, string password,
            Func<Stream, Task<string>>? captchaGenerator = null)
        {
            var loginInfo = await GetTwoPhaseLoginInfoAsync(username);

            var pwdDefaultEncryptSalt =
                loginInfo["password"];
            loginInfo["password"] = Encrypt(
                CryptoJsAes.GetRandomString(64) + password, pwdDefaultEncryptSalt);
            if (loginInfo.ContainsKey("captcha"))
            {
                if (captchaGenerator is null) throw new CaptchaRequiredException();


                var captchaStream =
                    await GetStreamAsync(loginInfo["captcha"]);
                loginInfo["captcha"] = await captchaGenerator(captchaStream);
            }

            await ApplyTwoPhaseLoginInfoAsync(loginInfo);
        }

        /// <summary>
        ///     获取进行两段CAS登录的登录信息(第一阶段)
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>
        /// 用于两段异步登录的登录信息字典.
        /// 其中password对应的value是加密的salt，
        /// captcha对应的value如果存在，则是用于获取验证码的url
        /// </returns>
        public async Task<Dictionary<string, string?>>
            GetTwoPhaseLoginInfoAsync(string username)
        {
            var htmlDoc = new HtmlDocument();

            htmlDoc.Load(await GetStreamAsync(LoginUrl));
            var loginInfoNode =
                htmlDoc.DocumentNode.SelectSingleNode(LoginInfoNodePath);
            var pwdDefaultEncryptSalt = loginInfoNode.SelectSingleNode("//input[@id='pwdEncryptSalt']")
                .GetAttributeValue("value", "");

            var captchaRequired = await GetStringAsync(
                $"{NeedCaptchaUrl}?username={username}");

            var captcha = captchaRequired == "{\"isNeed\":true}" ?
                $"{CaptchaUrl}?ts={(int)(DateTime.Now-new DateTime(1970,1,1)).TotalMilliseconds}" : null;

            string GetValue(string name)
            {
                return loginInfoNode.SelectSingleNode($"//input[@name='{name}']").GetAttributeValue("value", "");
            }

            var postContent = new Dictionary<string, string?>
            {
                { "username", username },
                { "password", pwdDefaultEncryptSalt }
            };
            if (captcha is { })
                postContent.Add("captcha", captcha);
            foreach (var key in new[]
            {
                "lt",
                "cllt",
                "dllt",
                "execution",
                "_eventId"
            })
                postContent.Add(key, GetValue(key));

            return postContent;
        }


        /// <summary>
        ///     应用两段CAS登录的登录信息进行登录(第二阶段)
        /// </summary>
        /// <param name="loginInfo">
        /// 来自<see cref="GetTwoPhaseLoginInfoAsync(string)"/>的登录信息，但是密码已加密，验证码(如需要)已填写
        /// </param>
        /// <returns></returns>
        /// <exception cref="LoginFailedException">登陆认证失败</exception>
        public async Task ApplyTwoPhaseLoginInfoAsync(
            Dictionary<string, string?> loginInfo)
        {

            if (!loginInfo.ContainsKey("captcha")) loginInfo.Add("captcha", "");
            loginInfo["cllt"] = "userNameLogin";

            var loginResponse = await PostAsync(LoginUrl, new FormUrlEncodedContent(
                loginInfo.Select(p => new KeyValuePair<string?, string?>(p.Key, p.Value)
                )));
    
            if (LoginEndpoints.All(x => x != loginResponse.RequestMessage?.RequestUri?.ToString()))
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(await loginResponse.Content.ReadAsStreamAsync());
                var xnode = htmlDoc.DocumentNode.SelectSingleNode(ErrorMessagePath);
                throw new LoginFailedException(
                    loginInfo.GetValueOrDefault("username") ?? "<UNKNOWN-USER>",
                    xnode is null? "<UNKNOWN-ERROR>" : xnode.InnerText);
            }
        }
    }
}