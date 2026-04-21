using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cloudflare_DDNS.Models;
using Cloudflare_DDNS.Interfaces;

public class CloudflareApi : ICloudflareApi
{
    private readonly HttpClient _httpClient;
    public CloudflareApi(HttpClient httpClient) => _httpClient = httpClient;
    /// <summary>
    /// Retrieves a list of DNS records for a specified zone and record type from the Cloudflare API. The method sends an HTTP GET request to the Cloudflare API endpoint, including the zone ID and record type as query parameters. It returns the response content as a string, which contains the list of DNS records in JSON format. If the request is successful, it ensures that the response status code indicates success; otherwise, it throws an exception.
    /// </summary>
    /// <param name="zoneId">The ID of the zone for which to list DNS records.</param>
    /// <param name="ApiToken">The API token for authentication with the Cloudflare API.</param>
    /// <param name="type">The type of DNS records to retrieve.</param>
    /// <returns>A string containing the JSON response with the list of DNS records.</returns>
    public async Task<string> ListDnsRecords(string zoneId, string ApiToken, DnsRecordType type = DnsRecordType.A)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiToken);

        var responseMessage = await _httpClient.GetAsync($"/client/v4/zones/{zoneId}/dns_records?per_page=200&type={type}");
        responseMessage.EnsureSuccessStatusCode();
        string content = await responseMessage.Content.ReadAsStringAsync();
        // Console.WriteLine(content);
        return content;
    }

    public async Task<string> ListZones(string ApiToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiToken);

        var responseMessage = await _httpClient.GetAsync($"/client/v4/zones?per_page=200");
        responseMessage.EnsureSuccessStatusCode();
        string content = await responseMessage.Content.ReadAsStringAsync();
        // Console.WriteLine(content);
        return content;
    }

    public Task<List<CloudflareZone>> MakeCloudflareZoneModelFromResponse(string responseContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var zoneResponse = JsonSerializer.Deserialize<CloudflareZoneResponse>(responseContent, options);
        if (zoneResponse is not null)
        {
            List<CloudflareZone> zones = zoneResponse.Result;
            // foreach (var item in zones)
            // {
            //     Console.WriteLine($"Zone ID: {item.Id}");
            //     Console.WriteLine($"Zone Name: {item.Name}");
            // }
            return Task.FromResult(zones);
        }
        else
        {
            Console.WriteLine("No zones found or failed to parse response.");
            return Task.FromResult(new List<CloudflareZone>());
        }
    }

    /// <summary>
    /// Parses the response content from the Cloudflare API and converts it into a list of DnsRecord objects.
    /// </summary>
    /// <param name="responseContent">The response content from the Cloudflare API.</param>
    /// <returns>a list of DnsRecord objects. returns an empty list if no records are found or if parsing fails.</returns>
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
    /// <summary>
    /// Updates a DNS record with the specified parameters. If the update is successful, it prints the response from the API. If the update fails, it throws an exception with the status code of the failed request.
    /// </summary>
    /// <param name="zoneId">The ID of the zone containing the DNS record to update.</param>
    /// <param name="apiToken">The API token for authentication with the Cloudflare API.</param>
    /// <param name="recordId">The ID of the DNS record to update.</param>
    /// <param name="name">The name of the DNS record.</param>
    /// <param name="type">The type of the DNS record.</param>
    /// <param name="content">The content of the DNS record.</param>
    /// <param name="ttl">The TTL of the DNS record.</param>
    /// <param name="proxied">Indicates whether the DNS record is proxied.</param>
    /// <param name="privateRouting">Indicates whether the DNS record uses private routing.</param>
    /// <param name="comment">A comment for the DNS record.</param>
    /// <returns></returns>
    /// <exception cref="Exception">Thrown when the update request fails.</exception>
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
    /// <summary>
    /// Checks if the DNS records need to be updated with the current IP and updates them if necessary. If dryRun is true, it will only print the changes that would be made without actually making any API calls to update the records.
    /// </summary>
    /// <param name="dnsrecords">The list of DNS records to check.</param>
    /// <param name="configRecords">The list of configuration records to compare against.</param>
    /// <param name="cloudflareConfig">The Cloudflare configuration.</param>
    /// <param name="currentIp">The current IP address.</param>
    /// <param name="dryRun">Indicates whether to perform a dry run (print changes without updating).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown when an error occurs while updating DNS records.</exception>
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
            }
        }
    }
}
