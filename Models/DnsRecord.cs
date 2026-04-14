namespace Cloudflare_DDNS.Models;

public class DnsRecord
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Proxiable { get; set; }
    public bool Proxied { get; set; }
    public int Ttl { get; set; }
    // public string settings { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public bool PrivateRouting { get; set; }
}