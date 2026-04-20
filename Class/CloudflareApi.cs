using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cloudflare_DDNS.Models;

public class CloudflareApi
{
    private readonly HttpClient _httpClient;
    public CloudflareApi(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<string> ListDnsRecords(string zoneId, string ApiToken, DnsRecordType type = DnsRecordType.A)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiToken);

        var responseMessage = await _httpClient.GetAsync($"/client/v4/zones/{zoneId}/dns_records?per_page=200&type={type}");
        responseMessage.EnsureSuccessStatusCode();
        string content = await responseMessage.Content.ReadAsStringAsync();
        // Console.WriteLine(content);
        return content;
    }

    public async Task<List<DnsRecord>> MakeDnsRecordModelFromResponse(string responseContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var dnsRecordResponse = JsonSerializer.Deserialize<CloudflareResponse>(responseContent, options);
        if (dnsRecordResponse is not null)
        {
            List<DnsRecord> dnsRecord = dnsRecordResponse.Result;
            // foreach (var item in dnsRecord)
            // {
            //     Console.WriteLine($"DNS Record ID: {item.Id}");
            //     Console.WriteLine($"DNS Record Name: {item.Name}");
            //     Console.WriteLine($"DNS Record Type: {item.Type}");
            //     Console.WriteLine($"DNS Record Content: {item.Content}");
            // }
            return dnsRecord;
        }
        else
        {
            Console.WriteLine("No DNS records found or failed to parse response.");
            return new List<DnsRecord>();
        }
    }

    public async Task UpdateDnsRecord(
        string zoneId,
        string apiToken,
        string recordId,
        string name,
        string type,
        string content,
        int ttl = 3600,
        bool proxied = true,
        bool privateRouting = true,
        string? comment = null)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);

        var payload = new
        {
            name,
            type,
            content,
            ttl,
            comment,
            proxied,
            private_routing = privateRouting
        };

        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(payload, options);
        using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var responseMessage = new HttpResponseMessage();
        try
        {
            responseMessage = await _httpClient.PatchAsync($"/client/v4/zones/{zoneId}/dns_records/{recordId}", httpContent);
            responseMessage.EnsureSuccessStatusCode();

            string responseBody = await responseMessage.Content.ReadAsStringAsync();
            Console.WriteLine($"DNS record updated successfully. statuscode: {responseMessage.StatusCode}");
            Console.WriteLine($"response: {responseBody}");
        }
        catch (System.Exception)
        {
            Console.WriteLine($"Failed to update DNS record. statuscode: {responseMessage.StatusCode}");
            throw new Exception($"Failed to update DNS record. statuscode: {responseMessage.StatusCode}");
        }

    }
    // TODO redo with foreach for multiple records
    public async Task UpdateRecordsIfNeeded(List<DnsRecord> dnsrecords, List<CloudflareConfigRecord> configRecords, CloudflareConfig cloudflareConfig, string currentIp, bool dryRun = true)
    {
        foreach (var configRecord in configRecords)
        {
            var matchingRecord = dnsrecords.FirstOrDefault(record => record.Name == configRecord.Name);
            if (matchingRecord != null)
            {
                if (matchingRecord.Content != currentIp)
                {
                    Console.WriteLine($"DNS record {matchingRecord.Name} needs to be updated from {matchingRecord.Content} to {currentIp}");
                    // Call UpdateDnsRecord method here with the appropriate parameters
                    if (!dryRun)
                    {
                        Console.WriteLine($"Updating DNS record {matchingRecord.Name} to new IP {currentIp}");
                        try
                        {
                            await UpdateDnsRecord(cloudflareConfig.ZoneId,
                            cloudflareConfig.ApiToken,
                            matchingRecord.Id,
                            matchingRecord.Name,
                            matchingRecord.Type,
                            currentIp,
                            matchingRecord.Ttl,
                            matchingRecord.Proxied,
                            matchingRecord.PrivateRouting,
                            matchingRecord.Comment);

                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine($"Failed to update DNS record {matchingRecord.Name} to new IP {currentIp}. Error: {ex.Message}");
                            throw new Exception($"Failed to update DNS record {matchingRecord.Name} to new IP {currentIp}");
                        }

                    }

                }
                else
                {
                    Console.WriteLine($"DNS record {matchingRecord.Name} is up to date with IP {currentIp}");
                }
            }
            else
            {
                Console.WriteLine($"No matching DNS record found for {configRecord.Name}. Consider creating a new record.");
                // Call CreateDnsRecord method here if you want to create a new record
            }
        }
    }
}
