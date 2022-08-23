using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall;

/// <summary>
///     从微信服务大厅查询到的课表数据
/// </summary>
public record WGScheduleEntry
{
    [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; }

    [JsonPropertyName("module")] public List<WGScheduleModule> Module { get; set; }
    [JsonPropertyName("msg")] public string Message { get; set; }
}

/// <summary>
///     查询课表数据返回的module部分
/// </summary>
public record WGScheduleModule
{
    /// <summary>
    ///     节次名称，如："1,2"
    /// </summary>
    [JsonPropertyName("jcmc")]
    public string CourseTimeExpr { get; set; }

    /// <summary>
    ///     周一的课程描述表达式
    /// </summary>
    [JsonPropertyName("mon")]
    public string MondayExpr{ get; set; }
    /// <summary>
    ///     周二的课程描述表达式
    /// </summary>
    [JsonPropertyName("tues")]
    public string TuesdayExpr { get; set; }
    /// <summary>
    ///     周3的课程描述表达式
    /// </summary>
    [JsonPropertyName("wed")]
    public string WednesdayExpr { get; set; }
    /// <summary>
    ///     周4课程描述表达式
    /// </summary>
    [JsonPropertyName("thur")]
    public string ThursdayExpr { get; set; }
    /// <summary>
    ///     周5的课程描述表达式
    /// </summary>
    [JsonPropertyName("fri")]
    public string FridayExpr { get; set; }
    /// <summary>
    ///     周6的课程描述表达式
    /// </summary>
    [JsonPropertyName("sat")]
    public string SaturdayExpr { get; set; }
    /// <summary>
    ///     周日的课程描述表达式
    /// </summary>
    [JsonPropertyName("sun")]
    public string SundayExpr { get; set; }
}