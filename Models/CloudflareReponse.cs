namespace Cloudflare_DDNS.Models;

public class CloudflareResponse
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Messages { get; set; } = new List<string>();
    public List<DnsRecord> Result { get; set; } = new List<DnsRecord>();
}