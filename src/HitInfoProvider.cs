﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace HitRefresh.HitGeneralServices;

/// <summary>
///     用户校园信息. 应由<see cref="HitInfoProviderFactory.GetHitInfo(ClaimsPrincipal)" /> 产生。
/// </summary>
public class HitInfoProvider
{
    private const string CasAuth = "casattras";


    internal HitInfoProvider(IDictionary<string, Claim> claims, HitInfoProviderOptions options)
    {
        if (!claims.TryGetValue(options.AuthMethodClaimType, out var authMethod)
            || authMethod.Value != CasAuth
            || !claims.TryGetValue(options.IdClaimType, out var id))
        {
            // 非统一身份认证的用户，Id都是空的。
            Role = HitRole.NotMember;
            Id = string.Empty;
            Name = string.Empty;
        }
        else
        {
            Id = id.Value;
            Role = HitRoleHelper.GetRole(Id);

            Name = claims.TryGetValue(options.NameClaimType, out var name)
                ? name.Value
                : string.Empty;
        }
    }

    /// <summary>
    ///     用户的校园角色
    /// </summary>
    public HitRole Role { get; }

    /// <summary>
    ///     用户的学/工号。如果是非校园成员，则为空串。
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     用户的姓名。如果是非校园成员，则为空串。
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     用于清除数据中的BUG：姓名双写。
    ///     因为历史遗留问题，部分用户的姓名是双写的，如：张三张三
    /// </summary>
    /// <returns>清除双写后的姓名。"张三"->"张三"；"张三张三"->"张三"</returns>
    private string CleanDoubleName(string originName)
    {
        // 奇数长度必然不是双写
        if ((originName.Length & 1) == 1) return originName;
        var hl = originName.Length >> 1;
        return originName[..hl] == originName[hl..]
            ? originName[..hl]
            : originName;
    }
}