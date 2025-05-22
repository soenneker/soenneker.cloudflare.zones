using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Cloudflare.Zones.Abstract;

namespace Soenneker.Cloudflare.Zones.Registrars;

/// <summary>
/// A .NET typesafe implementation of Cloudflare's Zones API
/// </summary>
public static class CloudflareZonesUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="ICloudflareZonesUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddCloudflareZonesUtilAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<ICloudflareZonesUtil, CloudflareZonesUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="ICloudflareZonesUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddCloudflareZonesUtilAsScoped(this IServiceCollection services)
    {
        services.TryAddScoped<ICloudflareZonesUtil, CloudflareZonesUtil>();

        return services;
    }
}
