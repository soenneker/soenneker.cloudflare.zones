using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Soenneker.Cloudflare.Zones.Abstract;
using Soenneker.Extensions.Configuration;
using Soenneker.Facts.Local;
using Soenneker.Tests.FixturedUnit;
using System.Threading.Tasks;
using Xunit;

namespace Soenneker.Cloudflare.Zones.Tests;

[Collection("Collection")]
public class CloudflareZonesUtilTests : FixturedUnitTest
{
    private readonly ICloudflareZonesUtil _util;
    private readonly IConfiguration _configuration;

    public CloudflareZonesUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ICloudflareZonesUtil>(true);
        _configuration = Resolve<IConfiguration>();
    }

    [Fact]
    public void Default()
    {
    }

    [LocalFact]
    public async ValueTask Add()
    {
        await _util.Add("clarityhealthguard.com", _configuration.GetValueStrict<string>("Cloudflare:AccountId"), CancellationToken);
    }

    [LocalFact]
    public async ValueTask GetNameservers()
    {
        var result = await _util.GetNameservers("clarityhealthguard.com", CancellationToken);
        result.Should().NotBeNullOrEmpty();
    }
}
