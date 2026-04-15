using System.Text.Json.Serialization;

namespace Cloudflare_DDNS.Models;


public class CloudflareConfigRecord
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    // public string Type { get; set; } = string.Empty;
}