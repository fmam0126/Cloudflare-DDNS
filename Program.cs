using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cloudflare_DDNS.Class;
using Cloudflare_DDNS.Interfaces;
using Cloudflare_DDNS.Models;
using System.Net.Http.Headers;


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
        List<CloudflareConfigRecord>? cloudflareConfigRecords;
        try
        {
            cloudflareConfig = config.GetSection("CloudflareConfig").Get<CloudflareConfig>();
            if (cloudflareConfig is null)
            {
                throw new Exception("CloudflareConfig section is missing in appsettings.json");
            }
            cloudflareConfigRecords = config.GetSection("CloudflareConfigRecord").Get<List<CloudflareConfigRecord>>();
            if (cloudflareConfigRecords is null || cloudflareConfigRecords.Count == 0)
            {
                throw new Exception("CloudflareConfigRecord section is missing or empty in appsettings.json");
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error parsing CloudflareConfig: {ex.Message}");
            return;
        }


        var builder = Host.CreateApplicationBuilder();

        builder.Services.ConfigureHttpClientDefaults(webBuilder =>
        {
            webBuilder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                ConnectTimeout = TimeSpan.FromSeconds(15),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            });
        });

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

        string lastIp = "1";
        string ip = string.Empty;
        while (true)
        {
            // get current ip address
            try
            {
                switch (cloudflareConfig.IpProvider)
                {
                    case GetIpProvider.CloudflareTrace:
                        ip = await getIp.GetIpWithCloudflareTrace(cloudflareConfig.CloudflareTraceUrl);
                        break;
                    case GetIpProvider.CloudflareGeolocationApi:
                        ip = await getIp.GetIpWithCloudflareGeolocationApi(cloudflareConfig.CloudflareGeolocationApiUrl);
                        break;
                    case GetIpProvider.Ipfy:
                        ip = await getIp.GetIpWithIpfy(cloudflareConfig.IpfyUrl);
                        break;
                    case GetIpProvider.Icanhazip:
                        ip = await getIp.GetIpWithicanhazip(cloudflareConfig.IcanhazipUrl);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cloudflareConfig.IpProvider), cloudflareConfig.IpProvider, null);
                }

            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"Invalid IP provider specified in configuration: {ex.Message}");
                break;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Failed to retrieve current IP address. error: {ex.Message}");
                Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
                await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
                continue;
            }

            if (ip == lastIp)
            {
                Console.WriteLine($"IP address has not changed. Current IP: {ip}, Last IP: {lastIp}");
            }
            else
            {
                Console.WriteLine($"IP address has changed. Current IP: {ip}, Last IP: {lastIp}");

                Console.WriteLine($"running update check for {cloudflareConfigRecords.Count} records...");
                List<DnsRecord> dnsRecords;

                try
                {
                    dnsRecords = await cloudflareApi.MakeDnsRecordModelFromResponse(await cloudflareApi.ListDnsRecords(cloudflareConfig.ZoneId, cloudflareConfig.ApiToken));

                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Failed to list DNS records from cloudflare. error: {ex.Message}");
                    Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
                    await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
                    continue;
                }

                try
                {
                    await cloudflareApi.UpdateRecordsIfNeeded(dnsRecords, cloudflareConfigRecords, cloudflareConfig, ip, dryRun: cloudflareConfig.DryRun);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Failed to update DNS records. error: {ex.Message}");
                    Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
                    await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
                    continue;
                }

                lastIp = ip;
            }
            Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
            await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
        }
    }
}
