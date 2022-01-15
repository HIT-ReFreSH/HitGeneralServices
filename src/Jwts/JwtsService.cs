using HitRefresh.HitGeneralServices.CasLogin;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HitRefresh.HitGeneralServices.Jwts
{
    /// <summary>
    /// 访问Jwts的服务
    /// </summary>
    public partial class JwtsService
    {
        private static readonly CultureInfo ChineseFormat = new CultureInfo("zh-CN");
        private LoginHttpClient httpClient = new();
        /// <summary>
        /// 查询个人课表
        /// </summary>
        /// <param name="year">学年</param>
        /// <param name="semester">学期</param>
        /// <returns>[7,6]的数组，7为周一至周日，6为节次；数组的每个元素是该位置的课程(课程可能会占用1-3行)</returns>
        public async Task<List<string>[,]> GetScheduleAsync(uint year, JwtsSemester semester)
        {
            const string url = "http://jwts.hit.edu.cn/kbcx/queryGrkb";
            const string scheduleTableNodePath = "/html/body/div[1]/div/div[8]/div[2]/table";
            var formData = new MultipartFormDataContent()
            {
                {new StringContent(semester switch
                {
                    JwtsSemester.Autumn=>$"{year}-{year+1}{(int)semester}",
                    _=>$"{year-1}-{year}{(int)semester}"
                }), "xnxq" },
                {new StringContent("kbcx/queryGrkb"),"fhlj" }
            };
            var response = await httpClient.PostAsync(url, formData);
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync());
            var scheduleTableNode = htmlDoc.DocumentNode.SelectSingleNode(scheduleTableNodePath);
            var scheduleTableRows = scheduleTableNode.SelectNodes("//tr").Skip(2).ToArray();
            var r = new List<string>[7,6];
            for (var i = 0; i < 6; i++)
            {
                var cells = scheduleTableRows[i].ChildNodes.Where(n => n.Name == "td").Skip(2).ToArray();
                for (int j = 0; j < 7; j++)
                {
                    r[j, i] = cells[j].ChildNodes.Where(n => n.Name == "#text")
                        .Select(n => n.InnerText).Where(t => t != "&nbsp").ToList();
                }
            }
            return r;

        }

        /// <summary>
        /// 采用给定的登录客户端(已经完成登录)，然后登录
        /// </summary>
        /// <param name="httpClient">登录客户端(已经完成登录)</param>
        /// <returns></returns>
        public async Task LoginAsync(LoginHttpClient httpClient)
        {
            this.httpClient = httpClient;
            await LoginAsync();

        }
        private async Task LoginAsync()
        {
            var resp = await httpClient.GetAsync("http://jwts.hit.edu.cn/loginCAS");
            resp= await httpClient.GetAsync("https://ids.hit.edu.cn/authserver/login?service=http%3A%2F%2Fjwts.hit.edu.cn%2FloginCAS");
            resp = await httpClient.GetAsync(resp.Headers.Location);
        }
        /// <summary>
        /// 使用给定的用户名与密码登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captchaGenerator">验证码输入回调</param>
        /// <returns></returns>
        public async Task LoginAsync(string username, string password, Func<Stream, Task<string>>? captchaGenerator = null)
        {

            await httpClient.TryLoginFor(username, password, 3, captchaGenerator);
            await LoginAsync();
        }
        /// <summary>
        /// 获取学期开始时间
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="semester">学期</param>
        /// <returns>开始时间</returns>
        public async Task<DateTime> GetSemesterStartAsync(uint year, JwtsSemester semester)
        {
            const string url = "http://jwts.hit.edu.cn/xlcx/queryXlcx";
            const string beginningNodePath = "/html/body/div/div/div[5]/div[1]/div";
            var formData = new MultipartFormDataContent()
            {
                {new StringContent(semester switch
                {
                    JwtsSemester.Autumn=>$"{year}-{year+1}{(int)semester}",
                    _=>$"{year-1}-{year}{(int)semester}"
                }), "xnxq" },
            };

            var response = await httpClient.PostAsync(url, formData);
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync());
            var beginningNode = htmlDoc.DocumentNode.SelectSingleNode(beginningNodePath);
            var monthExpr = beginningNode.ChildNodes
                .First(n => n.HasClass("xfyq_top"))?.InnerText;

            var dayExpr = beginningNode
                .SelectNodes("//td").First(n => n.HasClass("sk_green")).InnerText.Trim();

            return DateTime.Parse($"{monthExpr}{dayExpr}日", ChineseFormat);
        }
    }
}
