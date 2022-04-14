using System;

namespace HitRefresh.HitGeneralServices.Jwts;

/// <summary>
///     Jwts中的考试类型
/// </summary>
public enum JwtsExamType
{
    /// <summary>
    ///     期末
    /// </summary>
    Final = 1,

    /// <summary>
    ///     期中
    /// </summary>
    Middle = 2,

    /// <summary>
    ///     补考
    /// </summary>
    MakeUp = 3
}

/// <summary>
///     考试时间详细
/// </summary>
public record JwtsExamDetail
{
    /// <summary>
    ///     课程名称
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    ///     课程代码
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    ///     考试地点
    /// </summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    ///     考试时间
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    ///     长度
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    ///     周数
    /// </summary>
    public int WeekNumber { get; init; }

    /// <summary>
    ///     星期
    /// </summary>
    public DayOfWeek DayOfWeek { get; init; }

    /// <summary>
    ///     类型
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    ///     座位号
    /// </summary>
    public string SeatNumber { get; init; } = string.Empty;
}