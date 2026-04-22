using Cloudflare_DDNS.Models;
namespace Cloudflare_DDNS.Interfaces;

public interface ICloudflareApi
{
    Task<string> ListDnsRecords(string zoneId, string ApiToken, DnsRecordType type = DnsRecordType.A);
    Task<string> ListZones(string ApiToken);
    Task<List<DnsRecord>> MakeDnsRecordModelFromResponse(string responseContent);
    Task<List<CloudflareZone>> MakeCloudflareZoneModelFromResponse(string responseContent);
    Task UpdateDnsRecord(
        string zoneId,
        string apiToken,
        string recordId,
        string name,
        string type,
        string content,
        int ttl = 3600,
        bool proxied = true,
        bool privateRouting = true,
        string? comment = null);
    Task UpdateRecordsIfNeeded(List<DnsRecord> dnsrecords, List<CloudflareConfigRecord> configRecords, CloudflareConfig cloudflareConfig, string currentIp, bool dryRun = true);
}