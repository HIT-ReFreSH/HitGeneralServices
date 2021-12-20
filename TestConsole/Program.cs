using System;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.CasLogin;
using PlasticMetal.MobileSuit;
using HitRefresh.HitGeneralServices.Jwts;

namespace TestConsole
{
    internal class Program
    {
        public async void Login(string username, string password)
        {
            var server = new LoginHttpClient();
            await server.LoginAsync(username, password, LoginHttpClient.Win32CaptchaInput);
            var rep = await server.GetAsync("http://ids.hit.edu.cn/authserver/login");
            Console.WriteLine(rep.RequestMessage?.RequestUri?.ToString());
        }
        public async void JwtsTest(string username, string password)
        {
            var jwts = new JwtsService();
            await jwts.LoginAsync(username, password);
            Console.WriteLine(await jwts.GetScheduleAsync(2020, JwtsSemester.Autumn));
        }
        private static async Task Main(string[] args)
        {
            Suit.CreateBuilder().UsePowerLine().MapClient<Program>().Build().Run();
        }
    }
}