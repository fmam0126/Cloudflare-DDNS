using Cloudflare_DDNS.Models;
namespace Cloudflare_DDNS.Interfaces;

public interface ICloudflareApi
{
    Task<string> ListDnsRecords(string zoneId, string ApiToken, DnsRecordType type);
    Task<List<DnsRecord>> MakeDnsRecordModelFromResponse(string responseContent);
    Task UpdateDnsRecord(
        string zoneId,
        string apiToken,
        string recordId,
        string name,
        string type,
        string content,
        int ttl,
        bool proxied,
        bool privateRouting,
        string? comment);
    Task UpdateRecordsIfNeeded(List<DnsRecord> dnsrecords, List<CloudflareConfigRecord> configRecords, CloudflareConfig cloudflareConfig, string currentIp, bool dryRun);
    Task<string> ListZones(string ApiToken);
    Task<List<CloudflareZone>> MakeCloudflareZoneModelFromResponse(string responseContent);
}