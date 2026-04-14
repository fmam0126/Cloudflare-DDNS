using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cloudflare_DDNS.Class;
using Cloudflare_DDNS.Interfaces;
using Cloudflare_DDNS.Models;


namespace Cloudflare_DDNS;


class Program
{
    static async Task Main(string[] args)
    {
        IConfigurationRoot config;
        try
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./appsettings.json", optional: false)
                .Build();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return;
        }
        CloudflareConfig? cloudflareConfig;
        try
        {
            cloudflareConfig = config.GetSection("CloudflareConfig").Get<CloudflareConfig>();
            if (cloudflareConfig is null)
            {
                throw new Exception("CloudflareConfig section is missing in appsettings.json");
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error parsing CloudflareConfig: {ex.Message}");
            return;
        }


        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddHttpClient<CloudflareApi>(client =>
        {
            client.BaseAddress = new Uri("https://api.cloudflare.com/client/v4");
            client.DefaultRequestHeaders.Add("User-Agent", "Cloudflare-DDNS-client");
        });

        builder.Services.AddHttpClient<IGetIp, GetIp>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Cloudflare-DDNS-Client");
        });

        // Build and resolve services
        using var host = builder.Build();
        var getIp = host.Services.GetRequiredService<IGetIp>();
        var cloudflareApi = host.Services.GetRequiredService<CloudflareApi>();

        // Console.WriteLine(await GetIp.GetIpWithCloudflareTrace("https://one.one.one.one/cdn-cgi/trace"));
        var model = await getIp.GetIpWithCloudflareGeolocationApi(@"https://ipv4-check-perf.radar.cloudflare.com/api/info");

        Console.WriteLine(model.IpAddress);
        Console.WriteLine(await getIp.GetIpWithIpfy("https://api.ipify.org"));

        await cloudflareApi.ListDnsRecords(cloudflareConfig.ZoneId, cloudflareConfig.ApiToken);


        List<DnsRecord> dnsRecords = await cloudflareApi.MakeDnsRecordModelFromResponse(await cloudflareApi.ListDnsRecords(cloudflareConfig.ZoneId, cloudflareConfig.ApiToken));

        // var config = new CloudflareConfig
        // {
        //     ApiToken = "your_api_token",
        //     ZoneId = "your_zone_id",
        //     RecordId = "your_record_id"
        // };


    }
}
