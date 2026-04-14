using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
public class IpModel
{
    [JsonPropertyName("colo")]
    public string Colo { get; init; } = string.Empty;

    [JsonPropertyName("asn")]
    public int Asn { get; init; }

    [JsonPropertyName("continent")]
    public string Continent { get; init; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; init; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; init; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; init; } = string.Empty;

    [JsonPropertyName("latitude")]
    public string Latitude { get; init; } = string.Empty;

    [JsonPropertyName("longitude")]
    public string Longitude { get; init; } = string.Empty;

    [JsonPropertyName("ip_address")] // Maps the underscore to your property
    public string IpAddress { get; init; } = string.Empty;

    [JsonPropertyName("ip_version")]
    public string IpVersion { get; init; } = string.Empty;
}
public interface IGetIp
{
    Task<string> GetIpWithCloudflareTrace(string traceDomain);
    Task<IpModel> GetIpWithCloudflareGeolocationApi(string geolocationUrl);
    Task<string> GetIpWithIpfy(string ipifyDomain);
}

public class GetIp : IGetIp
{
    private readonly HttpClient _httpClient;

    public GetIp(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // this needs to be redone as the cloudflare trace domain is only for debugging purposes.

    /// <summary>
    /// gets ip using cloudflaretrace
    /// </summary>
    /// <param name="traceDomain">Cloudflare trace api domain</param>
    /// <returns>ip as a string or an empty string if ip isnt found</returns>
    public async Task<string> GetIpWithCloudflareTrace(string traceDomain)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(traceDomain);

        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();


        var lines = responseBody.Split('\n');
        // foreach (var line in lines)
        // {
        //     if (line.StartsWith("ip="))
        //     {
        //         string myip = line.Replace("ip=", "");
        //         Console.WriteLine(myip);
        //     }
        // }

        var traceData = responseBody.Split("\n", StringSplitOptions.None)
                    .Select(line => line.Split('='))
                    .Where(parts => parts.Length == 2)
                    .ToDictionary(
                        parts => parts[0].Trim(),
                        parts => parts[1].Trim()
                    );
        if (traceData.TryGetValue("ip", out string? ip))
        {
            // Console.WriteLine($"IP: {ip}");
            return ip;
        }
        Console.WriteLine($"Ip not found");
        return "";
    }
    /// <summary>
    /// gets ip using ipfy api
    /// </summary>
    /// <param name="ipifyDomain">Domain for the ipfy API</param>
    /// <returns>IP as a string or an empty string if IP isn't found</returns>
    public async Task<string> GetIpWithIpfy(string ipifyDomain)
    {
        var response = await _httpClient.GetAsync(ipifyDomain);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
    /// <summary>
    /// gets ip using the cloudflare geolocation api
    /// </summary>
    /// <param name="geolocationUrl">URL for the cloudflare geolocation API</param>
    /// <returns>IpModel instance TODO: return IP string</returns>
    public async Task<IpModel> GetIpWithCloudflareGeolocationApi(string geolocationUrl)
    {
        var response = await _httpClient.GetAsync(geolocationUrl);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        IpModel? ipModel = await JsonSerializer.DeserializeAsync<IpModel>(responseStream);

        return ipModel ?? new IpModel();
    }
}