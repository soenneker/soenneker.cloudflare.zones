using Soenneker.Cloudflare.Zones.Abstract;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Cloudflare.Utils.Client.Abstract;

namespace Soenneker.Cloudflare.Zones;

/// <inheritdoc cref="ICloudflareZonesUtil"/>
public sealed class CloudflareZonesUtil: ICloudflareZonesUtil
{
    private readonly ICloudflareClientUtil _clientUtil;

    public CloudflareZonesUtil(ICloudflareClientUtil clientUtil)
    {
        _clientUtil = clientUtil;
    }

    public async Task<CloudflareZone?> Get(string zoneId, CancellationToken cancellationToken = default)
    {
        var client = await _clientUtil.Get(cancellationToken);
        using var stream = await client.Zones[zoneId].GetAsync(cancellationToken: cancellationToken);
        if (stream == null) return null;
        return await JsonSerializer.DeserializeAsync<CloudflareZone>(stream, cancellationToken: cancellationToken);
    }
}
