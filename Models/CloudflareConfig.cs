
namespace Cloudflare_DDNS.Models;

public class CloudflareConfig
{
    public bool DryRun { get; set; } = false;
    public int IntervalMinutes { get; set; } = 5;
    public required string ApiToken { get; set; } = string.Empty;
    public required string ZoneId { get; set; } = string.Empty;
    public required GetIpProvider IpProvider { get; set; }
    public string CloudflareTraceUrl { get; set; } = "https://one.one.one.one/cdn-cgi/trace";
    public string CloudflareGeolocationApiUrl { get; set; } = "https://ipv4-check-perf.radar.cloudflare.com/api/info";
    public string IpfyUrl { get; set; } = "https://api.ipify.org";
    public string IcanhazipUrl { get; set; } = "https://ipv4.icanhazip.com/";
}