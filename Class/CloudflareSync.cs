using Cloudflare_DDNS.Models;

namespace Cloudflare_DDNS.Class;

public static class CloudflareSync
{
    /// <summary>
    /// This method takes a list of DNS records from Cloudflare, the Cloudflare configuration, and an array of Cloudflare configuration records. It checks if there are matching DNS records for each configuration record and prints the results. You can expand this method to include logic for updating or creating DNS records as needed.
    /// </summary>
    /// <param name="dnsRecords">a list of DNS records from Cloudflare</param>
    /// <param name="cloudflareConfig">the Cloudflare configuration</param>
    /// <param name="cloudflareConfigRecords">an array of Cloudflare records from the configuration</param>
    public static void SyncDnsRecords(List<DnsRecord> dnsRecords, CloudflareConfig cloudflareConfig, List<CloudflareConfigRecord> cloudflareConfigRecords)
    {
        foreach (var configRecord in cloudflareConfigRecords)
        {
            var matchingRecord = dnsRecords.FirstOrDefault(r => r.Name == configRecord.Name && r.Type == configRecord.Type);
            if (matchingRecord != null)
            {
                Console.WriteLine($"Found matching record: {matchingRecord.Name} ({matchingRecord.Type}) with content {matchingRecord.Content}");
                // Here you would add logic to update the DNS record if needed
                
            }
            else
            {
                Console.WriteLine($"No matching record found for {configRecord.Name} ({configRecord.Type})");
                // Here you would add logic to create a new DNS record if needed
            }
        }
    }
}