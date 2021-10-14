using HtmlAgilityPack;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HitRefresh.HitGeneralServices.CasLogin
{
    /// <summary>
    /// 可以CAS登录的HttpClient
    /// </summary>
    public class LoginHttpClient: HttpClient
    {
        /// <summary>
        /// 使用Windows默认图片查看器查看验证码图片
        /// </summary>
        /// <param name="s">验证码的源头流</param>
        /// <returns>验证码结果</returns>
        public static async Task<string> WindowsCaptchaInput(Stream s)
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
            return Console.ReadLine()??"";
        }
        private string Username { get; }
        private string Password { get; }
        private const string LoginInfoNodePath = "/html/body/div[2]/div[2]/div[2]/div/div[3]/div/form/div";
        private const string ErrorMessagePath = "/html/body/div[2]/div[2]/div[2]/div/div[3]/div/form/span";
        private const string CaptchaUrl = "http://ids.hit.edu.cn/authserver/captcha.html";
        private const string NeedCaptchaUrl = "http://ids.hit.edu.cn/authserver/needCaptcha.html";
        private const string LoginUrl = "http://ids.hit.edu.cn/authserver/login";
        private const string LoginEndpoint = "http://ids.hit.edu.cn/authserver/index.do";
        /// <summary>
        /// Implementation of _rds function in encrypt.js
        /// </summary>
        /// <param name="length">length of the RandomString</param>
        /// <returns></returns>
        private static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";
            var rnd = new Random();
            var sb=new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length-1)]);
            }
            return sb.ToString();
        }

        private class CryptoJsAes:IDisposable
        {
            private Aes Aes{ get; } = Aes.Create();

            public CryptoJsAes(string strPassword)
            {
                Aes.Key=Encoding.UTF8.GetBytes(strPassword);
                Aes.IV = Encoding.UTF8.GetBytes(GetRandomString(16));
                Aes.Padding = PaddingMode.PKCS7;
                Aes.Mode = CipherMode.CBC;
            }

            public string Encrypt(string strPlainText)
            {
                var strText = new UTF8Encoding().GetBytes(strPlainText);
                var transform = Aes.CreateEncryptor();
                var cipherText = transform.TransformFinalBlock(strText, 0, strText.Length);

                return Convert.ToBase64String(cipherText);
            }

            public string Decrypt(string encryptedText)
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var decryptor = Aes.CreateDecryptor();
                var originalBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(originalBytes);
            }


            public void Dispose()
            {
                this.Aes.Dispose();
            }
        }
        /// <summary>
        /// Implementation of encryptAES function in encrypt.js
        /// </summary>
        /// <param name="message">message to encode with AES and BASE64</param>
        /// <param name="passPhrase">pass phrase for AES</param>
        /// <returns></returns>
        private static string Encrypt(string message, string passPhrase)
        {
            using var cryptoJs = new CryptoJsAes(passPhrase);
            return cryptoJs.Encrypt(message);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public LoginHttpClient(string username,string password)
        {
            Username = username;
            Password = password;
            MaxResponseContentBufferSize = 256000;
            DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");
        }
        /// <summary>
        /// 使用给定的用户名和密码，进行CAS登录
        /// </summary>
        /// <param name="captchaGenerator">用于生成填写二维码的适配器</param>
        /// <returns></returns>
        /// <exception cref="CaptchaRequiredException"></exception>
        /// <exception cref="LoginFailedException"></exception>
        public async Task LoginAsync(Func<Stream,Task<string>>? captchaGenerator=null)
        {
            var htmlDoc = new HtmlDocument();

            htmlDoc.Load(await GetStreamAsync(LoginUrl));
            var loginInfoNode =
                htmlDoc.DocumentNode.SelectSingleNode(LoginInfoNodePath);
            var pwdDefaultEncryptSalt = loginInfoNode.SelectSingleNode("//input[@id='pwdDefaultEncryptSalt']")
                .GetAttributeValue("value","");
            string? captcha=null;
            var passwordEncrypt = Encrypt(
                GetRandomString(64) + Password, pwdDefaultEncryptSalt);
            var captchaRequired = await GetStringAsync(
                $"{NeedCaptchaUrl}?username={Username}&pwdEncrypt2={pwdDefaultEncryptSalt}");
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
                return loginInfoNode.SelectSingleNode($"//input[@name='{name}']").GetAttributeValue("value","");
            }

            var postContent = new Dictionary <string, string?>
            {
                { "username", Username },
                { "password", passwordEncrypt }
            };
            if (captcha is {} )
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
                postContent.Select(p=>new KeyValuePair<string?, string?>(p.Key,p.Value)
                )));

            if (loginResponse.RequestMessage?.RequestUri?.ToString()!=LoginEndpoint)
            {
                htmlDoc = new();
                htmlDoc.Load(await loginResponse.Content.ReadAsStreamAsync());
                throw new LoginFailedException(Username,
                    htmlDoc.DocumentNode.SelectSingleNode(ErrorMessagePath).GetDirectInnerText());
                
            }

        }
    }
}
