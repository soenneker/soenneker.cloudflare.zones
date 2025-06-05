using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Soenneker.Cloudflare.OpenApiClient.Models;

namespace Soenneker.Cloudflare.Zones.Abstract;

/// <summary>
/// Provides utilities for managing Cloudflare zones such as adding, checking, retrieving, and removing zones.
/// </summary>
public interface ICloudflareZonesUtil
{
    /// <summary>
    /// Adds a new site to Cloudflare.
    /// </summary>
    /// <param name="domainName">The domain name to add (e.g., "example.com").</param>
    /// <param name="accountId">The Cloudflare account ID.</param>
    /// <param name="cancellationToken">Optional cancellation token for the request.</param>
    /// <returns>A task that returns the ID of the newly created zone.</returns>
    ValueTask<string> Add(string domainName, string accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a site exists in Cloudflare.
    /// </summary>
    /// <param name="domainName">The domain name to check.</param>
    /// <param name="cancellationToken">Optional cancellation token for the request.</param>
    /// <returns>A task that returns true if the site exists, false otherwise.</returns>
    ValueTask<bool> Exists(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Cloudflare zone ID for a specified domain.
    /// </summary>
    /// <param name="domainName">The domain name to look up.</param>
    /// <param name="cancellationToken">Optional cancellation token for the request.</param>
    /// <returns>A task that returns the zone ID if found, or throws an exception if not found.</returns>
    ValueTask<string> GetId(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a site from Cloudflare.
    /// </summary>
    /// <param name="domainName">The domain name to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token for the request.</param>
    /// <returns>A task that returns true if the site was successfully removed, or false if it did not exist.</returns>
    ValueTask<bool> Remove(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed information about a zone.
    /// </summary>
    /// <param name="domainName">The domain name to get information for.</param>
    /// <param name="cancellationToken">Optional cancellation token for the request.</param>
    /// <returns>A task that returns the zone details.</returns>
    ValueTask<Zones_apiResponseSingleId> Get(string domainName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the nameservers for a zone.
    /// </summary>
    /// <param name="domainName">The domain name to get nameservers for.</param>
    /// <param name="cancellationToken">Optional cancellation token for the request.</param>
    /// <returns>A task that returns a list of nameservers.</returns>
    ValueTask<IReadOnlyList<string>> GetNameservers(string domainName, CancellationToken cancellationToken = default);
}