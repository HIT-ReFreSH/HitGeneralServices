using System;
using HitRefresh.HitGeneralServices;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     用于依赖注入的扩展方法
/// </summary>
public static class HitGeneralServicesExtensions
{
    /// <summary>
    ///     添加HitInfoProviderFactory到服务集合
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="setupAction">配置HitInfoProvider的选项</param>
    /// <returns></returns>
    public static IServiceCollection AddHitInfoProviderFactory(this IServiceCollection services,
        Action<HitInfoProviderOptions>? setupAction = null)
    {
        var options = new HitInfoProviderOptions();
        setupAction?.Invoke(options);
        services.AddSingleton(new HitInfoProviderFactory(options));
        return services;
    }
}