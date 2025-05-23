using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Cloudflare.Zones.Abstract;
using System;
using System.IO;
using System.Threading.Tasks;
using Soenneker.Utils.Json;
using System.Threading;

namespace Soenneker.Cloudflare.Zones;

///<inheritdoc cref="ICloudflareZonesUtil"/>
public sealed class CloudflareZonesUtil : ICloudflareZonesUtil
{
    private readonly ILogger<CloudflareZonesUtil> _logger;
    private readonly ICloudflareClientUtil _clientUtil;

    public CloudflareZonesUtil(ILogger<CloudflareZonesUtil> logger, ICloudflareClientUtil clientUtil)
    {
        _logger = logger;
        _clientUtil = clientUtil;
    }

    public async ValueTask<string> Add(string domainName, string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be empty", nameof(accountId));

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            _logger.LogInformation("Adding site {DomainName} to Cloudflare", domainName);

            var request = new Zones_post_RequestBody_application_json
            {
                Name = new Zones_name {Value = domainName},
                Account = new Zones_post_RequestBody_application_json_account
                {
                    Id = new Zones_identifier {Value = accountId}
                },
                Type = new Zones_type {Value = Zones_type_Value.Full}
            };

            Stream? response = await client.Zones.PostAsync(request, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new Exception("Failed to add site - no response received");
            }

            var responseObj = await JsonUtil.Deserialize<Zones_apiResponseSingleId>(response, _logger, cancellationToken);
            if (responseObj?.Success != true)
            {
                throw new Exception("Failed to add site - API call was not successful");
            }

            if (responseObj.Result?.Id?.Value == null)
            {
                throw new Exception("Failed to add site - no zone ID in response");
            }

            _logger.LogInformation("Successfully added site {DomainName} with zone ID {ZoneId}", domainName, responseObj.Result.Id.Value);
            return responseObj.Result.Id.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding site {DomainName} to Cloudflare", domainName);
            throw;
        }
    }

    public async ValueTask<bool> Exists(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            _logger.LogInformation("Checking if site {DomainName} exists in Cloudflare", domainName);

            var request = new Zones_get_RequestBody_application_json();
            Stream? response = await client.Zones.GetAsync(request, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new Exception("Failed to check site existence - no response received");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if site {DomainName} exists in Cloudflare", domainName);
            throw;
        }
    }

    public async ValueTask<string> GetId(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            _logger.LogInformation("Getting zone ID for domain {DomainName}", domainName);

            var request = new Zones_get_RequestBody_application_json();
            Stream? response = await client.Zones.GetAsync(request, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new Exception("Failed to get zone ID - no response received");
            }

            var responseObj = await JsonUtil.Deserialize<Zones_apiResponseCommon>(response, _logger, cancellationToken);
            if (responseObj?.Success != true)
            {
                throw new Exception("Failed to get zone ID - API call was not successful");
            }

            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting zone ID for domain {DomainName}", domainName);
            throw;
        }
    }

    public async ValueTask<bool> Remove(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

        try
        {
            _logger.LogInformation("Removing site {DomainName} from Cloudflare", domainName);

            string? zoneId = await GetId(domainName, cancellationToken);
            if (zoneId == null)
            {
                _logger.LogWarning("Site {DomainName} not found in Cloudflare", domainName);
                return false;
            }

            var request = new Zones_0_delete_RequestBody_application_json();
            Zones_apiResponseSingleId? response = await client.Zones[zoneId].DeleteAsync(request, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new Exception("Failed to remove site - no response received");
            }

            var responseObj = response as Zones_apiResponseSingleId;
            if (responseObj?.Success != true)
            {
                throw new Exception("Failed to remove site - API call was not successful");
            }

            _logger.LogInformation("Successfully removed site {DomainName}", domainName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing site {DomainName} from Cloudflare", domainName);
            throw;
        }
    }
}