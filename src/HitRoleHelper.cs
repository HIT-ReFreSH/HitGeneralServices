using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HitRefresh.HitGeneralServices;

/// <summary>
/// 提供角色有关的工具类
/// </summary>
public static class HitRoleHelper
{
    private static readonly Regex RegexTeacherId = new(@"^[0-9]{8}$");
    private static readonly Regex RegexInternationalStudentId = new(@"^[Ll][0-9]{9}$");
    private static readonly Regex RegexUndergraduateId = new(@"^([0-9]{10})|(1[0-9]{2}[Ll][0-9]{6})$");
    private static readonly Regex RegexMasterId = new(@"^[0-9]{2}[Ss][0-9]{6}$");
    private static readonly Regex RegexDoctorId = new(@"^[0-9]{2}[Bb][0-9]{6}$");
    /// <summary>
    /// 根据学工号提取角色
    /// </summary>
    /// <param name="hitCasId">学工号，也是CAS登陆的用户名</param>
    /// <returns>角色</returns>
    public static HitRole GetRole(string hitCasId)
    {
        return RegexUndergraduateId.IsMatch(hitCasId)
            ? HitRole.Undergraduate
            : RegexInternationalStudentId.IsMatch(hitCasId)
                ? HitRole.InternationalStudent
                : RegexTeacherId.IsMatch(hitCasId)
                    ? HitRole.Teacher
                    : RegexMasterId.IsMatch(hitCasId)
                        ? HitRole.Master
                        : RegexDoctorId.IsMatch(hitCasId)
                            ? HitRole.Doctor
                            : HitRole.Else;
    }
}