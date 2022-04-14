using System;
using System.Security.Cryptography;
using System.Text;

namespace HitRefresh.HitGeneralServices.CasLogin;

public partial class LoginHttpClient
{
    private class CryptoJsAes : IDisposable
    {
        public CryptoJsAes(string strPassword)
        {
            Aes.Key = Encoding.UTF8.GetBytes(strPassword);
            Aes.IV = Encoding.UTF8.GetBytes(GetRandomString(16));
            Aes.Padding = PaddingMode.PKCS7;
            Aes.Mode = CipherMode.CBC;
        }

        private Aes Aes { get; } = Aes.Create();


        public void Dispose()
        {
            Aes.Dispose();
        }

        /// <summary>
        ///     获取一段指定长度的随机字符串
        /// </summary>
        /// <param name="length">随机字符串长度</param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";
            var rnd = new Random();
            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++) sb.Append(chars[rnd.Next(chars.Length - 1)]);
            return sb.ToString();
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
    }
}