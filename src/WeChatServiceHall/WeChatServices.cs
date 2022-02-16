using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.Jwts;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall
{
    /// <summary>
    /// 微信服务大厅的服务
    /// </summary>
    public class WeChatServices
    {
        private static readonly CultureInfo ChineseFormat = new CultureInfo("zh-CN");
        public static async Task<List<WScheduleEntry>> GetScheduleAnonymousAsync(
            uint year, JwtsSemester semester, string studentId
            )
        {
            const string url = "https://wxfwdt.hit.edu.cn/app/bkskbcx/kbcxapp/getBkszkb";
            HttpClient httpClient = new();
            var r=new List<WScheduleEntry>();
            for (var i=1;;i++)
            {
                var data = new Dictionary<string, string>
                {
                    {
                        "info", JsonSerializer.Serialize(new Dictionary<string, string>
                        {
                            {"gxh", studentId},
                            {
                                "xnxq", semester switch
                                {
                                    JwtsSemester.Autumn => $"{year}-{year + 1};{(int) semester}",
                                    _ => $"{year - 1}-{year};{(int) semester}"
                                }
                            },
                            {"zc", $"{i}"}
                        })
                    }
                };

                var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(data)); ;
                var result = await
                    JsonSerializer.DeserializeAsync<WScheduleEntry>(await response.Content.ReadAsStreamAsync());
                if (result?.Module.Dates.Any()==true) r.Add(result);
                else break;
            }

            return r;
        }
    }
}
