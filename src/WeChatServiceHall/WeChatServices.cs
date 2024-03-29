﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HitRefresh.HitGeneralServices.Jwts;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall;

/// <summary>
///     微信服务大厅的服务
/// </summary>
public class WeChatServices
{
    private static readonly CultureInfo ChineseFormat = new("zh-CN");

    private static IEnumerable<int> GetWeekIndexSequence()
    {
        for (var i = 1; i <= 20; i++) yield return i;
    }
    /// <summary>
    /// 获取研究生课表
    /// </summary>
    /// <param name="year"></param>
    /// <param name="semester"></param>
    /// <param name="studentId"></param>
    /// <returns></returns>
    public static async Task<List<WScheduleEntry>> 
        GetGraduateScheduleAnonymousAsync(
        uint year, JwtsSemester semester, string studentId
    )
    {
        const string url = "https://wxfwdt.hit.edu.cn/app/kbcx/kbcxapp/getYjszkb";
        // 一个本科生学号，用于查询校历
        const string underGraduateId = "1180100101";
        var underGraduateData = await GetScheduleAnonymousAsync(year, semester, underGraduateId);
        HttpClient httpClient = new();
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
                    }
                })
            }
        };

        var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(data));
        var result = await
                JsonSerializer.DeserializeAsync<WGScheduleEntry>(await response.Content.ReadAsStreamAsync());

        return WGScheduleUtils.MergeGraduate(underGraduateData, result);
    }
    /// <summary>
    ///     匿名获取课表
    /// </summary>
    /// <param name="year"></param>
    /// <param name="semester"></param>
    /// <param name="studentId"></param>
    /// <returns></returns>
    public static async Task<List<WScheduleEntry>> GetScheduleAnonymousAsync(
        uint year, JwtsSemester semester, string studentId
    )
    {
        var role = HitRoleHelper.GetRole(studentId);
        if (role is HitRole.Master or HitRole.Doctor)
            return await GetGraduateScheduleAnonymousAsync(year, semester, studentId);
        const string url = "https://wxfwdt.hit.edu.cn/app/bkskbcx/kbcxapp/getBkszkb";
        HttpClient httpClient = new();
        var r = new WScheduleEntry?[21];

        async Task Get(int i)
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

            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(data));
            try
            {
                var result = await
                    JsonSerializer.DeserializeAsync<WScheduleEntry>(await response.Content.ReadAsStreamAsync());
                if (result?.Module.Dates.Any() == true) r[i] = result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        await Task.WhenAll(GetWeekIndexSequence().Select(Get));


        return r.Skip(1).OfType<WScheduleEntry>().ToList();
    }
}