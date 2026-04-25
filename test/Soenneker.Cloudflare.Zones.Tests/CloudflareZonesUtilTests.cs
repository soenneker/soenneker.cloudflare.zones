using System.Collections.Generic;
using System.Threading;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Soenneker.Cloudflare.Zones.Abstract;
using Soenneker.Extensions.Configuration;
using Soenneker.Tests.HostedUnit;
using System.Threading.Tasks;

namespace Soenneker.Cloudflare.Zones.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class CloudflareZonesUtilTests : HostedUnitTest
{
    private readonly ICloudflareZonesUtil _util;
    private readonly IConfiguration _configuration;

    public CloudflareZonesUtilTests(Host host) : base(host)
    {
        _util = Resolve<ICloudflareZonesUtil>(true);
        _configuration = Resolve<IConfiguration>();
    }

    [Test]
    public void Default()
    {
    }

    [Skip("Manual")]
    // [LocalOnly]
    public async ValueTask Add(CancellationToken cancellationToken)
    {
        await _util.Add("", _configuration.GetValueStrict<string>("Cloudflare:AccountId"), cancellationToken);
    }

    [Skip("Manual")]
    // [LocalOnly]
    public async ValueTask GetNameservers(CancellationToken cancellationToken)
    {
        List<string> result = await _util.GetNameservers("", cancellationToken);
        result.Should()
              .NotBeNullOrEmpty();
    }
}
