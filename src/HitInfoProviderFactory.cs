using System.Linq;
using System.Security.Claims;

namespace HitRefresh.HitGeneralServices;

/// <summary>
///     用于产生用户校园信息的工厂
/// </summary>
public class HitInfoProviderFactory
{
    private readonly HitInfoProviderOptions _options;

    internal HitInfoProviderFactory(HitInfoProviderOptions options)
    {
        _options = options;
    }

    /// <summary>
    ///     获取给定用户的校园信息
    /// </summary>
    /// <param name="user">当前用户</param>
    /// <returns>用户的校园信息</returns>
    public HitInfoProvider GetHitInfo(ClaimsPrincipal user)
    {
        return new HitInfoProvider(user.Claims.ToDictionary(c => c.Type), _options);
    }
}