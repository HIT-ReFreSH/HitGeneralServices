using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall;

/// <summary>
///     从微信服务大厅查询到的课表数据
/// </summary>
public record WScheduleEntry
{
    [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; }

    [JsonPropertyName("module")] public WScheduleModule Module { get; set; }
}

/// <summary>
///     查询课表数据返回的module部分
/// </summary>
public record WScheduleModule
{
    /// <summary>
    ///     日期数据
    /// </summary>
    [JsonPropertyName("rqData")]
    public WTimeEntry[] Dates { get; set; }

    /// <summary>
    ///     课程数据
    /// </summary>
    [JsonPropertyName("data")]
    public List<WCourseEntry> Courses { get; set; }
}