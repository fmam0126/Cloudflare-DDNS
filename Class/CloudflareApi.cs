using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class CloudflareApi
{
    private readonly HttpClient _httpClient;
    public CloudflareApi(HttpClient httpClient) => _httpClient = httpClient;

    public async Task ListDnsRecords(string zoneId, string ApiToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiToken);
        var responseMessage = await _httpClient.GetAsync($"/client/v4/zones/{zoneId}/dns_records");
        responseMessage.EnsureSuccessStatusCode();
        string content = await responseMessage.Content.ReadAsStringAsync();
        Console.WriteLine(content);
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