using System.Text.Json.Serialization;

namespace Cloudflare_DDNS.Models;

public class IpModel
{
    [JsonPropertyName("colo")]
    public string Colo { get; init; } = string.Empty;

    [JsonPropertyName("asn")]
    public int Asn { get; init; }

    [JsonPropertyName("continent")]
    public string Continent { get; init; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; init; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; init; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; init; } = string.Empty;

    [JsonPropertyName("latitude")]
    public string Latitude { get; init; } = string.Empty;

    [JsonPropertyName("longitude")]
    public string Longitude { get; init; } = string.Empty;

    [JsonPropertyName("ip_address")] // Maps the underscore to your property
    public string IpAddress { get; init; } = string.Empty;

    [JsonPropertyName("ip_version")]
    public string IpVersion { get; init; } = string.Empty;
}