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
public static class GetIp
{
    static readonly HttpClient httpClient = new HttpClient();

    // this needs to be redone as the cloudflare trace domain is only for debugging purposes.


    /// <summary>
    /// gets ip using cloudflaretrace
    /// </summary>
    /// <param name="TraceDomain">Cloudflare trace api domain</param>
    /// <returns>ip as a string or an empty string if ip isnt found</returns>
    public static async Task<string> GetIpWithCloudflareTrace(string TraceDomain)
    {

        HttpResponseMessage response = await httpClient.GetAsync(TraceDomain);

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

    public static async Task<IpModel> GetIpWithCloudflareGeolocationApi(string GeolocationUrl)
    {

        var response = await httpClient.GetAsync(GeolocationUrl);
        response.EnsureSuccessStatusCode();

        // var responseBody = response.Content;

        using var responseStream = await response.Content.ReadAsStreamAsync();
        IpModel? ipModel = await JsonSerializer.DeserializeAsync<IpModel>(responseStream);


        return ipModel;
    }
}