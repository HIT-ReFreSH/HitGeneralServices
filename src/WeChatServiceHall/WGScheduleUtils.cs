using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HitRefresh.HitGeneralServices.WeChatServiceHall;
/// <summary>
/// 
/// </summary>
/// <param name="CourseName">课程名称</param>
/// <param name="Values">老师 地点 周数三元组</param>
public record WGCourseEntry(string CourseName, List<(string Teacher, string Location, int Week)> Values)
{
    private static Regex TeacherNameRegex { get; } = new(@"^[\u4e00-\u9fa5^0-9]{2,4}$|^(\w+\s?)+$");


    private static Regex CourseTimeRegex { get; } = new(@"^\[(((\d+)|((\d+)\-(\d+)))(单|双)?(\|)?)+\](单|双)?$");

    private static Regex LocationRegex { get; } = new(@"^([\u4e00-\u9fa5]+|[A-Z]{1,2})\d{2,5}$");

    private static Regex ScheduleExpressionUnitRegex { get; } =
        new(
            @"(([\u4e00-\u9fa5]+|[A-Z]{1,2})\d{2,5})|(\[(((\d+)|((\d+)\-(\d+)))(单|双)?(\|)?)+\](单|双)?)|([\u4e00-\u9fa5]{2,4}|(\w+\s?)+)");
    /// <summary>
    ///     表达式单元的种类
    /// </summary>
    private enum ScheduleExpressionUnitType
    {
        /// <summary>
        ///     教师
        /// </summary>
        Teacher = 0,

        /// <summary>
        ///     时间信息
        /// </summary>
        Time = 1,

        /// <summary>
        ///     教室信息
        /// </summary>
        Location = 2,

        /// <summary>
        ///     未知，出错了
        /// </summary>
        Unknown = -1
    }
    private static string RemoveCommaSpace(string source)
    {
        return source
            .Replace("单周", "单", StringComparison.CurrentCultureIgnoreCase)
            .Replace("双周", "双", StringComparison.CurrentCultureIgnoreCase)
            .Replace("周]", "]", StringComparison.CurrentCultureIgnoreCase) //移出时间表达式后面的“周”
            .Replace("]周", "]", StringComparison.CurrentCultureIgnoreCase) //移出时间表达式后面的“周”
            .Replace(", ", "|", true, CultureInfo.CurrentCulture) //英文逗号+空格
            .Replace("，", "|", true, CultureInfo.CurrentCulture); //中文逗号
    }
    private static IEnumerable<int> ToIntSequence(string source)
    {
        if (!CourseTimeRegex.IsMatch(source)
            || source == null) throw new ArgumentOutOfRangeException(nameof(source), source, null);
        var r = new List<int>();
        var subWeekExpression = source.Split('|');

        foreach (var s in subWeekExpression)
        {
            var hasSingle = !s.Contains('双', StringComparison.CurrentCultureIgnoreCase);
            var hasDouble = !s.Contains('单', StringComparison.CurrentCultureIgnoreCase);


            var weekRange =
                Regex.Matches(s, @"\d+").AsParallel()
                    .Select(w => int.Parse(w.Value, CultureInfo.CurrentCulture.NumberFormat))
                    .ToList();


            if (weekRange.Count == 0) continue;
            if (weekRange.Count == 1)
                r.Add(weekRange[0]);
            else
                for (var i = weekRange[0]; i <= weekRange[1]; i++)
                    if (hasDouble && (i & 1) == 0 ||
                        hasSingle && (i & 1) == 1)
                        r.Add(i);
        }

        return source[^1] switch
        {
            '单' => r.AsParallel().Where(i => (i & 1) == 1).ToList(),
            '双' => r.AsParallel().Where(i => (i & 1) == 0).ToList(),
            _ => r
        };
    }

    public static WGCourseEntry[] FromExpressions(string expr)
        => expr.Split("<br/>").Where(str => !string.IsNullOrEmpty(str))
            .Select(FromExpression).ToArray();
    public static WGCourseEntry FromExpression(string expr)
    {
        // 句尾符号
        var tailIdx = expr.IndexOf('<');
        // 此处去除句尾的HTML
        expr = tailIdx <= 0 ? expr : expr[..tailIdx];
        var seq = expr.Split('◇');
        tailIdx = seq.Length > 2 ? seq[2].LastIndexOf('[') : 0;
        // 此处去除[xx节]，然后Parse Week Expression
        var exprBody = RemoveCommaSpace(seq.Length > 2 ? seq[1] + (tailIdx <= 0 ? seq[2] : seq[2][..tailIdx]) : seq[1]);
        // 兼容本科格式
        var courseName = seq[0].Replace('[', '(').Replace(']', ')');

        var currentTeacher = "";
        var timeStack = new Stack<string>();
        var timeTeacherMap = new Dictionary<string, string>();
        var timeLocationMap = new Dictionary<string, string>();

        foreach (var match in ScheduleExpressionUnitRegex.Matches(exprBody))
        {
            var unit = match?.ToString();
            if (unit == null)
                continue;
            var unitType = LocationRegex.IsMatch(unit) ? ScheduleExpressionUnitType.Location :
                TeacherNameRegex.IsMatch(unit) ? ScheduleExpressionUnitType.Teacher :
                CourseTimeRegex.IsMatch(unit) ? ScheduleExpressionUnitType.Time :
                ScheduleExpressionUnitType.Unknown;

            switch (unitType)
            {
                case ScheduleExpressionUnitType.Teacher:
                    currentTeacher = unit;
                    break;
                case ScheduleExpressionUnitType.Time:
                    timeTeacherMap.Add(unit, currentTeacher);
                    timeStack.Push(unit);
                    break;
                case ScheduleExpressionUnitType.Location:
                    while (timeStack.Count > 0) timeLocationMap.Add(timeStack.Pop(), unit);
                    break;
                case ScheduleExpressionUnitType.Unknown:
                    throw new ArgumentException(exprBody, nameof(exprBody), null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitType), unitType.ToString());
            }
        }

        while (timeStack.Count > 0) timeLocationMap.Add(timeStack.Pop(), "<地点待定>");
        var values = timeTeacherMap.Keys.SelectMany(time =>
            ToIntSequence(time).Select(
                weekIndex => (timeTeacherMap[time], timeLocationMap[time], weekIndex))).ToList();
        return new(courseName, values);
    }

}
public class WGScheduleUtils
{
    /// <summary>
    /// 合并当前学期的本科生与研究生课表
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="graduate"></param>
    /// <returns></returns>
    public static List<WScheduleEntry> MergeGraduate(List<WScheduleEntry> origin, WGScheduleEntry graduate)
    {
        origin.ForEach(e => e.Module.Courses.Clear());
        foreach (var module in graduate.Module)
        {
            var weekdayExprMap = new[]
            {
                module.MondayExpr,
                module.TuesdayExpr,
                module.WednesdayExpr,
                module.ThursdayExpr,
                module.FridayExpr,
                module.SaturdayExpr,
                module.SundayExpr
            };
            for (var i = 0; i < 7; i++)
            {
                var dayOfWeek = i + 1;
                foreach (var (courseName, valueTuples) in WGCourseEntry.FromExpressions(weekdayExprMap[i]))
                {
                    foreach (var (teacher,loc,week) in valueTuples)
                    {
                        origin[week - 1].Module.Courses.Add(new()
                        {
                            Name = courseName,
                            DayOfWeek = dayOfWeek.ToString(),
                            CourseTime = $"第{module.CourseTimeExpr}节",
                            Location = loc,
                            Teacher = teacher
                        });
                    }
                }
            }
        }

        return origin;
    }
}