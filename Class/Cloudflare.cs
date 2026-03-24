public class CloudflareApi
{
        private readonly HttpClient _httpClient;
        public CloudflareApi(HttpClient httpClient) => _httpClient = httpClient;
        public async Task ListDnsRecords(IHttpClientFactory httpClientFactory, string zoneId)
    {
        await _httpClient.GetAsync($"/zones/{zoneId}/dns_records");
    }
}