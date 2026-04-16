using System.Data;

namespace Cloudflare_DDNS.Models;
public class CloudflareConfig
{
    public bool DryRun { get; set; } = false;
    public int IntervalMinutes { get; set; } = 5;
    public required string ApiToken { get; set; } = string.Empty;
    public required string ZoneId { get; set; } = string.Empty;
}