using System.Data;

namespace Cloudflare_DDNS.Models;
public class CloudflareConfig
{
    public string ApiToken { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    // public string RecordId { get; set; } = string.Empty;
}