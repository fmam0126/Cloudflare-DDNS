using System.Net;

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
    public async Task UpdateDnsRecord(string zoneId, string apiToken, string recordId)
    {

    }
}