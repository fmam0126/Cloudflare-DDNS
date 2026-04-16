using System.Diagnostics;

public class GetIpDomains
{
    public string TraceDomain { get; init; } = "https://one.one.one.one/cdn-cgi/trace";
    public string GeolocationUrl { get; init; } = "https://ipv4-check-perf.radar.cloudflare.com/api/info";
    public string IpfyDomain { get; init; } = "https://api.ipify.org";
    public string IcanhazipDomain { get; init; } = "https://icanhazip.com";
}