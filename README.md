[![](https://img.shields.io/nuget/v/soenneker.cloudflare.zones.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.zones/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.zones/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.zones/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.cloudflare.zones.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.cloudflare.zones/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.cloudflare.zones/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.cloudflare.zones/actions/workflows/codeql.yml)

# Soenneker.Cloudflare.Zones

Creates, finds, inspects, and deletes Cloudflare zones and retrieves their assigned nameservers.

## Installation

```bash
dotnet add package Soenneker.Cloudflare.Zones
```

## Configuration

```json
{
  "Cloudflare": {
    "ApiKey": "your-api-token"
  }
}
```

The token needs zone-read permission for lookup operations and zone-edit permission for creation or deletion.

## Registration

```csharp
using Soenneker.Cloudflare.Zones.Registrars;

services.AddCloudflareZonesUtilAsScoped();
```

Singleton registration is available with `AddCloudflareZonesUtilAsSingleton()`.

## Lookup

```csharp
using Soenneker.Cloudflare.Zones.Abstract;

bool exists = await zones.Exists("example.com", cancellationToken);
string? zoneId = await zones.GetId("example.com", cancellationToken);

if (zoneId is not null)
{
    List<string> nameservers =
        await zones.GetNameservers("example.com", cancellationToken);
}
```

`Get` returns the generated `ZonesZone` model and throws `CloudflareApiException` when no matching zone is returned or Cloudflare reports a failure. `GetId` returns `null` only for a genuine missing-zone result; authentication, transport, and other Cloudflare failures are not treated as absence.

`Get(domainName, apiKey)` and `GetId(domainName, apiKey)` use a token supplied for that call. Other methods use `Cloudflare:ApiKey`.

## Create and delete

```csharp
string zoneId = await zones.Add("example.com", accountId, cancellationToken);

bool removed = await zones.Remove("example.com", cancellationToken);
```

`Add` creates a full zone in the specified account. The returned nameservers still need to be configured at the domain's registrar before Cloudflare becomes authoritative.

`Remove` first resolves the zone by exact domain name and returns `false` if it does not exist. Otherwise it permanently deletes the zone and returns `true`; DNS records and zone configuration are removed with it. Cloudflare failures throw `CloudflareApiException`, while cancellation propagates as `OperationCanceledException`.
