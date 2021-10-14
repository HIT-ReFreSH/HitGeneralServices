using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.CasLogin;
using Microsoft.VisualBasic;
using PlasticMetal.MobileSuit;

namespace TestConsole
{
    internal class Program
    {
        public async void Login(string username, string password)
        {
            var server = new LoginHttpClient(username, password);
            await server.LoginAsync(LoginHttpClient.WindowsCaptchaInput);
            var rep = await server.GetAsync("http://ids.hit.edu.cn/authserver/login");
            Console.WriteLine(rep.RequestMessage?.RequestUri?.ToString());
        }
        static async Task Main(string[] args)
        {
            Suit.GetBuilder().UsePowerLine().Build<Program>().Run();
        }
    }
}
