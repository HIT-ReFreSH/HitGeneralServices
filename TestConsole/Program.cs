﻿using System;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.CasLogin;
using PlasticMetal.MobileSuit;

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

        private static async Task Main(string[] args)
        {
            Suit.GetBuilder().UsePowerLine().Build<Program>().Run();
        }
    }
}