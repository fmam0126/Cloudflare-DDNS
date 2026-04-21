using System.Text.Json.Serialization;

namespace Cloudflare_DDNS.Models;

public class CloudflareZone
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}