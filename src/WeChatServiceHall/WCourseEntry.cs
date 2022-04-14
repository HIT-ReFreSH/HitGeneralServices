using System.Text.Json.Serialization;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall;

/// <summary>
///     用于解析微信课表条目的记录
/// </summary>
public record WCourseEntry
{
    /// <summary>
    ///     上课地点
    /// </summary>
    [JsonPropertyName("cdmc")]
    public string Location { get; set; }

    /// <summary>
    ///     上课教师
    /// </summary>
    [JsonPropertyName("jsxm")]
    public string Teacher { get; set; }

    /// <summary>
    ///     课程名称
    /// </summary>
    [JsonPropertyName("kcmc")]
    public string Name { get; set; }

    /// <summary>
    ///     上课时间，形如“第9,10节”“第1,2节”
    /// </summary>
    [JsonPropertyName("sksjms")]
    public string CourseTime { get; set; }

    /// <summary>
    ///     星期，1-7的整数
    /// </summary>
    [JsonPropertyName("xqj")]
    public string DayOfWeek { get; set; }
}