using System;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.CasLogin;
using PlasticMetal.MobileSuit;
using HitRefresh.HitGeneralServices.Jwts;

namespace TestConsole
{
    internal class Program
    {
        [SuitAlias("qk")]
        public async void GetScheduleAnonymousAsync(string username)
        {
            var r = await HitRefresh.HitGeneralServices.WeChatServices.GetScheduleAnonymousAsync(
                2022, JwtsSemester.Spring, username);
            
            Console.WriteLine(r.Count);
        }
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
            Console.WriteLine(await jwts.GetSemesterStartAsync(2020, JwtsSemester.Autumn));
            Console.WriteLine((await jwts.GetExamDetailsAsync())[0]);
        }
        private static async Task Main(string[] args)
        {
            Suit.CreateBuilder().UsePowerLine().MapClient<Program>().Build().Run();
        }
    }
}