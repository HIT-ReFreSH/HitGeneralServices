using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HitRefresh.HitGeneralServices.Jwts;

public partial class JwtsService
{
    private static async Task<JwtsExamDetail[]> GetExamDetailsAsync(HttpContent content)
    {
        const string scheduleTableNodePath = "/html/body/div/div/div[5]/table";
        var htmlDoc = new HtmlDocument();

        htmlDoc.LoadHtml(await content.ReadAsStringAsync());
        var scheduleTableNode = htmlDoc.DocumentNode.SelectSingleNode(scheduleTableNodePath);
        var scheduleTableRows = scheduleTableNode.SelectNodes("//tr")
            .Skip(2).ToArray();

        return scheduleTableRows.Select(ParseJwtsExamDetail).ToArray();
    }

    /// <summary>
    ///     获取考试详细(指定学期和类别)
    /// </summary>
    /// <param name="year"></param>
    /// <param name="semester"></param>
    /// <param name="type"></param>
    /// <returns>一组考试详细</returns>
    public async Task<JwtsExamDetail[]> GetExamDetailsAsync(
        uint year, JwtsSemester semester, JwtsExamType type)
    {
        const string url = "http://jwts.hit.edu.cn/kscx/queryKcForXs";
        var formData = new MultipartFormDataContent
        {
            {
                new StringContent(semester switch
                {
                    JwtsSemester.Autumn => $"{year}-{year + 1}{(int) semester}",
                    _ => $"{year - 1}-{year}{(int) semester}"
                }),
                "xnxq"
            },
            {new StringContent($"{(int) type:00}"), "kssjd"}
        };
        var response = await httpClient.PostAsync(url, formData);

        return await GetExamDetailsAsync(response.Content);
    }

    /// <summary>
    ///     获取考试详细(默认学期)
    /// </summary>
    /// <returns>一组考试详细</returns>
    public async Task<JwtsExamDetail[]> GetExamDetailsAsync()
    {
        const string url = "http://jwts.hit.edu.cn/kscx/queryKcForXs";

        var response = await httpClient.GetAsync(url);

        return await GetExamDetailsAsync(response.Content);
    }

    private static JwtsExamDetail ParseJwtsExamDetail(HtmlNode node)
    {
        var cells = node.ChildNodes.Where(n => n.Name == "td").Skip(1).ToArray();
        var location = cells[2].InnerText.Trim();
        var timeCellExpressions = cells[4].InnerText.Trim().Split('(', ')', ' ', '-');
        var date = DateTime.Parse(timeCellExpressions[0], ChineseFormat);
        var weekNumber = int.Parse(timeCellExpressions[1][1..^1]);
        var beginTime = TimeSpan.Parse($"{timeCellExpressions[^2]}:00");
        var endTime = TimeSpan.Parse($"{timeCellExpressions[^1]}:00");
        return new JwtsExamDetail
        {
            Name = cells[0].InnerText.Trim(),
            Code = cells[1].InnerText.Trim(),
            Location = location.Split('-')[^1],
            SeatNumber = cells[3].InnerText.Trim(),
            StartTime = date.Add(beginTime),
            Duration = endTime - beginTime,
            DayOfWeek = date.DayOfWeek,
            WeekNumber = weekNumber,
            Type = cells[5].InnerText.Trim()
        };
    }
}