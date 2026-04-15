using System.Net;
using System.Net.Http.Json;
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
        var responseMessage = await _httpClient.GetAsync($"/client/v4/zones/{zoneId}/dns_records?type={type}");
        responseMessage.EnsureSuccessStatusCode();
        string content = await responseMessage.Content.ReadAsStringAsync();
        Console.WriteLine(content);
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
            var dnsRecord = dnsRecordResponse.Result;
            Console.WriteLine($"DNS Record ID: {dnsRecord[0].Id}");
            Console.WriteLine($"DNS Record Name: {dnsRecord[0].Name}");
            Console.WriteLine($"DNS Record Type: {dnsRecord[0].Type}");
            Console.WriteLine($"DNS Record Content: {dnsRecord[0].Content}");
            foreach (var item in dnsRecord)
            {
                Console.WriteLine($"DNS Record ID: {item.Id}");
                Console.WriteLine($"DNS Record Name: {item.Name}");
                Console.WriteLine($"DNS Record Type: {item.Type}");
                Console.WriteLine($"DNS Record Content: {item.Content}");
            }
            return dnsRecord;
        }
        else
        {
            Console.WriteLine("No DNS records found or failed to parse response.");
            return null;
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

        var responseMessage = await _httpClient.PatchAsync($"/client/v4/zones/{zoneId}/dns_records/{recordId}", httpContent);
        responseMessage.EnsureSuccessStatusCode();

        string responseBody = await responseMessage.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
    }
}