using Soenneker.Cloudflare.Zones.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Cloudflare.Zones.Tests;

[Collection("Collection")]
public class CloudflareZonesUtilTests : FixturedUnitTest
{
    private readonly ICloudflareZonesUtil _util;

    public CloudflareZonesUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ICloudflareZonesUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
