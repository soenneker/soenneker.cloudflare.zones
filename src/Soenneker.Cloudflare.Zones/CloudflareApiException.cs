using System;

namespace Soenneker.Cloudflare.Zones;

/// <summary>
/// Custom exception for Cloudflare API errors
/// </summary>
public class CloudflareApiException : Exception
{
    public string DomainName { get; }

    public CloudflareApiException(string message, string domainName, Exception? innerException = null) 
        : base(message, innerException)
    {
        DomainName = domainName;
    }
} 