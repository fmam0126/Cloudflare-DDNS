using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cloudflare_DDNS.Models;
using Cloudflare_DDNS.Interfaces;
using Microsoft.VisualBasic;

namespace Cloudflare_DDNS.Class;

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
    public async Task<string> GetIpWithCloudflareGeolocationApi(string geolocationUrl)
    {
        var response = await _httpClient.GetAsync(geolocationUrl);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        IpModel? ipModel = await JsonSerializer.DeserializeAsync<IpModel>(responseStream);

        return ipModel?.IpAddress ?? string.Empty;
    }

    public async Task<string> GetIpWithicanhazip(string icanhazipDomain)
    {
        var response = await _httpClient.GetAsync(icanhazipDomain);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();

    }
}