namespace Cloudflare_DDNS.Models;

public class CloudflareZoneResponse
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Messages { get; set; } = new List<string>();
    public List<CloudflareZone> Result { get; set; } = new List<CloudflareZone>();
}