using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cloudflare_DDNS;


class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddHttpClient<CloudflareApi>(client => {
            client.BaseAddress = new Uri("https://api.cloudflare.com/client/v4");
            client.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory");
        });
        // Add services to the container.
        

        // Console.WriteLine(await GetIp.GetIpWithCloudflareTrace("https://one.one.one.one/cdn-cgi/trace"));
        var model = await GetIp.GetIpWithCloudflareGeolocationApi(@"https://ipv4-check-perf.radar.cloudflare.com/api/info");

        Console.WriteLine(model.IpAddress);

        var config = new CloudflareConfig
        {
            ApiToken = "your_api_token",
            ZoneId = "your_zone_id",
            RecordId = "your_record_id"
        };

        
    }
}
