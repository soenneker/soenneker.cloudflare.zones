using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Soenneker.Cloudflare.Zones.Abstract;
using Soenneker.Extensions.Configuration;
using Soenneker.Tests.HostedUnit;
using System.Threading.Tasks;
using Soenneker.Facts.Manual;

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

    [ManualFact]
    // [LocalOnly]
    public async ValueTask Add()
    {
        await _util.Add("", _configuration.GetValueStrict<string>("Cloudflare:AccountId"), CancellationToken);
    }

    [ManualFact]
    // [LocalOnly]
    public async ValueTask GetNameservers()
    {
        List<string> result = await _util.GetNameservers("", CancellationToken);
        result.Should()
              .NotBeNullOrEmpty();
    }
}