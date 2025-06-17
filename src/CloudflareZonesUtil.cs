using Microsoft.Extensions.Logging;
using Soenneker.Cloudflare.OpenApiClient;
using Soenneker.Cloudflare.OpenApiClient.Models;
using Soenneker.Cloudflare.OpenApiClient.Zones;
using Soenneker.Cloudflare.Utils.Client.Abstract;
using Soenneker.Cloudflare.Zones.Abstract;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kiota.Abstractions;
using Soenneker.Extensions.ValueTask;
using Soenneker.Cloudflare.OpenApiClient.Zones.Item;

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

        // Validate domain name format
        if (!Uri.CheckHostName(domainName).Equals(UriHostNameType.Dns))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        _logger.LogInformation("Adding site {DomainName} to Cloudflare", domainName);

        try
        {
            CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

            var request = new Zones_post
            {
                Name = domainName,
                Account = new Zones_post_account
                {
                    Id = accountId
                },
                Type = Zones_type.Full
            };

            var response = await client.Zones.PostAsync(request, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new CloudflareApiException("Failed to add site - no response received", domainName);
            }

            if (response.Success != true)
            {
                var error = response.Errors?.FirstOrDefault();
                var errorMessage = error?.Message ?? "Unknown error";
                var errorCode = error?.Code?.ToString() ?? "Unknown";
                throw new CloudflareApiException($"Failed to add site - API call was not successful. Error: {errorCode} - {errorMessage}", domainName);
            }

            if (response.Result?.Id == null)
            {
                throw new CloudflareApiException("Failed to add site - no zone ID in response", domainName);
            }

            _logger.LogInformation("Successfully added site {DomainName} with zone ID {ZoneId}", domainName, response.Result.Id);
            return response.Result.Id;
        }
        catch (Zones_apiResponseCommonFailure failure)
        {
            _logger.LogError(failure, "Cloudflare API error adding site {DomainName}. Request details: AccountId={AccountId}", domainName, accountId);
            throw new CloudflareApiException($"Failed to add site {domainName}: {failure.Message}", domainName, failure);
        }
        catch (Exception ex) when (ex is not CloudflareApiException)
        {
            _logger.LogError(ex, "Error adding site {DomainName} to Cloudflare", domainName);
            throw new CloudflareApiException($"Error adding site {domainName} to Cloudflare", domainName, ex);
        }
    }

    public async ValueTask<bool> Exists(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        // Validate domain name format
        if (!Uri.CheckHostName(domainName).Equals(UriHostNameType.Dns))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        _logger.LogInformation("Checking if site {DomainName} exists in Cloudflare", domainName);

        try
        {
            CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

            var requestConfig = new Action<RequestConfiguration<ZonesRequestBuilder.ZonesRequestBuilderGetQueryParameters>>(config =>
            {
                config.QueryParameters.Name = domainName;
            });

            var response = await client.Zones.GetAsync(requestConfig, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new CloudflareApiException("Failed to check site existence - no response received", domainName);
            }

            if (response.Success != true)
            {
                var error = response.Errors?.FirstOrDefault();
                var errorMessage = error?.Message ?? "Unknown error";
                var errorCode = error?.Code?.ToString() ?? "Unknown";
                throw new CloudflareApiException($"Failed to check site existence - API call was not successful. Error: {errorCode} - {errorMessage}", domainName);
            }

            bool exists = response.Result?.Any() == true;
            
            _logger.LogInformation("Site {DomainName} {Exists} in Cloudflare", domainName, exists ? "exists" : "does not exist");
            return exists;
        }
        catch (Zones_apiResponseCommonFailure failure)
        {
            _logger.LogError(failure, "Cloudflare API error checking site existence for {DomainName}", domainName);
            throw new CloudflareApiException($"Failed to check site existence for {domainName}: {failure.Message}", domainName, failure);
        }
        catch (Exception ex) when (ex is not CloudflareApiException)
        {
            _logger.LogError(ex, "Error checking if site {DomainName} exists in Cloudflare", domainName);
            throw new CloudflareApiException($"Error checking if site {domainName} exists in Cloudflare", domainName, ex);
        }
    }

    public async ValueTask<string?> GetId(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        // Validate domain name format
        if (!Uri.CheckHostName(domainName).Equals(UriHostNameType.Dns))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        _logger.LogInformation("Getting zone ID for domain {DomainName}", domainName);

        try
        {
            var zone = await Get(domainName, cancellationToken);
            if (zone?.Id == null)
            {
                _logger.LogWarning("No matching zone found for domain {DomainName}", domainName);
                return null;
            }

            _logger.LogInformation("Successfully retrieved zone ID {ZoneId} for domain {DomainName}", zone.Id, domainName);
            return zone.Id;
        }
        catch (CloudflareApiException)
        {
            _logger.LogWarning("No matching zone found for domain {DomainName}", domainName);
            return null;
        }
    }

    public async ValueTask<bool> Remove(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        // Validate domain name format
        if (!Uri.CheckHostName(domainName).Equals(UriHostNameType.Dns))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        _logger.LogInformation("Removing site {DomainName} from Cloudflare", domainName);

        try
        {
            CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

            string? zoneId = await GetId(domainName, cancellationToken);
            if (zoneId == null)
            {
                _logger.LogWarning("Site {DomainName} not found in Cloudflare", domainName);
                return false;
            }

            var body = new Zone_identifierDeleteRequestBody();
            var response = await client.Zones[zoneId].DeleteAsync(body, null, cancellationToken);
            if (response == null)
            {
                throw new CloudflareApiException("Failed to remove site - no response received", domainName);
            }

            if (response.Success != true)
            {
                var error = response.Errors?.FirstOrDefault();
                var errorMessage = error?.Message ?? "Unknown error";
                var errorCode = error?.Code?.ToString() ?? "Unknown";
                throw new CloudflareApiException($"Failed to remove site - API call was not successful. Error: {errorCode} - {errorMessage}", domainName);
            }

            _logger.LogInformation("Successfully removed site {DomainName}", domainName);
            return true;
        }
        catch (Zones_apiResponseCommonFailure failure)
        {
            _logger.LogError(failure, "Cloudflare API error removing site {DomainName}", domainName);
            throw new CloudflareApiException($"Failed to remove site {domainName}: {failure.Message}", domainName, failure);
        }
        catch (Exception ex) when (ex is not CloudflareApiException)
        {
            _logger.LogError(ex, "Error removing site {DomainName} from Cloudflare", domainName);
            throw new CloudflareApiException($"Error removing site {domainName} from Cloudflare", domainName, ex);
        }
    }

    public async ValueTask<Zones_zone> Get(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        // Validate domain name format
        if (!Uri.CheckHostName(domainName).Equals(UriHostNameType.Dns))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        _logger.LogInformation("Getting zone details for domain {DomainName}", domainName);

        try
        {
            CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken);

            var requestConfig = new Action<RequestConfiguration<ZonesRequestBuilder.ZonesRequestBuilderGetQueryParameters>>(config =>
            {
                config.QueryParameters.Name = domainName;
            });

            var response = await client.Zones.GetAsync(requestConfig, cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new CloudflareApiException("Failed to get zone details - no response received", domainName);
            }

            if (response.Success != true)
            {
                var error = response.Errors?.FirstOrDefault();
                var errorMessage = error?.Message ?? "Unknown error";
                var errorCode = error?.Code?.ToString() ?? "Unknown";
                throw new CloudflareApiException($"Failed to get zone details - API call was not successful. Error: {errorCode} - {errorMessage}", domainName);
            }

            var zone = response.Result?.FirstOrDefault();
            if (zone?.Id == null)
            {
                throw new CloudflareApiException($"Zone not found for domain {domainName}", domainName);
            }

            _logger.LogInformation("Successfully retrieved zone details for domain {DomainName} with ID {ZoneId}", domainName, zone.Id);
            return zone;
        }
        catch (Zones_apiResponseCommonFailure failure)
        {
            _logger.LogError(failure, "Cloudflare API error getting zone details for {DomainName}", domainName);
            throw new CloudflareApiException($"Failed to get zone details for {domainName}: {failure.Message}", domainName, failure);
        }
        catch (Exception ex) when (ex is not CloudflareApiException)
        {
            _logger.LogError(ex, "Error getting zone details for domain {DomainName}", domainName);
            throw new CloudflareApiException($"Error getting zone details for domain {domainName}", domainName, ex);
        }
    }

    public async ValueTask<List<string>> GetNameservers(string domainName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        // Validate domain name format
        if (!Uri.CheckHostName(domainName).Equals(UriHostNameType.Dns))
            throw new ArgumentException("Invalid domain name format", nameof(domainName));

        _logger.LogInformation("Getting nameservers for domain {DomainName}", domainName);

        try
        {
            CloudflareOpenApiClient client = await _clientUtil.Get(cancellationToken).NoSync();

            string? zoneId = await GetId(domainName, cancellationToken);
            if (string.IsNullOrEmpty(zoneId))
            {
                throw new CloudflareApiException($"Zone not found for domain {domainName}", domainName);
            }

            var response = await client.Zones[zoneId].GetAsync(cancellationToken: cancellationToken);
            if (response == null)
            {
                throw new CloudflareApiException("Failed to get zone details - no response received", domainName);
            }

            if (response.Success != true)
            {
                var error = response.Errors?.FirstOrDefault();
                var errorMessage = error?.Message ?? "Unknown error";
                var errorCode = error?.Code?.ToString() ?? "Unknown";
                throw new CloudflareApiException($"Failed to get zone details - API call was not successful. Error: {errorCode} - {errorMessage}", domainName);
            }

            if (response.Result?.NameServers == null || !response.Result.NameServers.Any())
            {
                throw new CloudflareApiException("Failed to get nameservers - no nameservers in response", domainName);
            }

            List<string> nameserverList = response.Result.NameServers.ToList();

            _logger.LogInformation("Successfully retrieved {Count} nameservers for domain {DomainName}: {Nameservers}", 
                nameserverList.Count, domainName, string.Join(", ", nameserverList));

            return nameserverList;
        }
        catch (Zones_apiResponseCommonFailure failure)
        {
            _logger.LogError(failure, "Cloudflare API error getting nameservers for {DomainName}", domainName);
            throw new CloudflareApiException($"Failed to get nameservers for {domainName}: {failure.Message}", domainName, failure);
        }
        catch (Exception ex) when (ex is not CloudflareApiException)
        {
            _logger.LogError(ex, "Error getting nameservers for domain {DomainName}", domainName);
            throw new CloudflareApiException($"Error getting nameservers for domain {domainName}", domainName, ex);
        }
    }
}