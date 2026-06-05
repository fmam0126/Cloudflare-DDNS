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
                .AddJsonFile("./appsettings.json", optional: true)
                .AddEnvironmentVariables()
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
            cloudflareConfigRecords = config.GetSection("CloudflareConfigRecord").Get<List<CloudflareConfigRecord>>();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error parsing CloudflareConfig: {ex.Message}");
            return;
        }
        string csvRecords = Environment.GetEnvironmentVariable("DnsRecords") ?? string.Empty;

        if (!string.IsNullOrEmpty(csvRecords))
        {
            cloudflareConfigRecords = csvRecords.Split(",").Select(record => new CloudflareConfigRecord
            {
                Name = record.Trim()
            }).ToList();
        }

        if (cloudflareConfig is null)
        {
            Console.WriteLine("CloudflareConfig section is missing or invalid");
            return;
        }
        if (cloudflareConfigRecords is null || cloudflareConfigRecords.Count == 0)
        {
            Console.WriteLine("CloudflareConfigRecord section is missing or invalid");
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

        builder.Services.AddHttpClient<ICloudflareApi, CloudflareApi>(client =>
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
        var cloudflareApi = host.Services.GetRequiredService<ICloudflareApi>();

        string lastIp = string.Empty;
        string ip = string.Empty;

        List<CloudflareZone> zones;
        try
        {
            zones = await cloudflareApi.MakeCloudflareZoneModelFromResponse(await cloudflareApi.ListZones(cloudflareConfig.ApiToken));
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Failed to list Cloudflare zones. Error: {ex.Message}");
            return;
        }

        // loop through zones and records to find matching zone for each record, then make a map of zone to records for later use in update logic. 
        // this is needed to support multiple zones and records that may belong to different zones.
        var zoneToRecordMap = new Dictionary<string, List<CloudflareConfigRecord>>();

        foreach (var zone in zones)
        {
            Console.WriteLine($"Zone ID: {zone.Id}, Zone Name: {zone.Name}");
            foreach (var record in cloudflareConfigRecords)
            {
                if (record.Name.EndsWith(zone.Name, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Record {record.Name} matches zone {zone.Name}");
                    if (!zoneToRecordMap.ContainsKey(zone.Id))
                    {
                        zoneToRecordMap[zone.Id] = new List<CloudflareConfigRecord>();
                    }
                    zoneToRecordMap[zone.Id].Add(record);


                }

                // string recordDomainName = record.Name.Substring(record.Name.IndexOf('.') + 1);
                // if (recordDomainName.Equals(zone.Name, StringComparison.OrdinalIgnoreCase))
                // {
                //     Console.WriteLine($"Record {record.Name} matches zone {zone.Name}");
                //     cloudflareConfig.ZoneId = zone.Id;

                //     break;
                // }
            }
        }

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
            // check if ip is valid public ipv4 address
            if (!getIp.IsValidPublicIp4(ip))
            {
                Console.WriteLine($"Retrieved IP address is not a valid IPv4 address: {ip}");
                Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
                await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
                continue;
            }
            // compare with last ip address, if changed then update cloudflare dns record
            if (ip == lastIp)
            {
                Console.WriteLine($"IP address has not changed. Current IP: {ip}, Last IP: {lastIp}");
            }
            else
            {
                Console.WriteLine($"IP address has changed. Current IP: {ip}, Last IP: {lastIp}");

                // List<DnsRecord> dnsRecords;
                // TODO: Update here for multiple zone support.
                foreach (var entry in zoneToRecordMap)
                {
                    string zoneId = entry.Key;
                    var recordsInThisZone = entry.Value;
                    Console.WriteLine($"running update check for {cloudflareConfigRecords.Count} records...");
                    try
                    {
                        // lists dns records in this zone and makes a model out of the response
                        var response = await cloudflareApi.ListDnsRecords(zoneId, cloudflareConfig.ApiToken);
                        List<DnsRecord> currentDnsRecords = await cloudflareApi.MakeDnsRecordModelFromResponse(response);

                        // runs update logic for all records in this zone
                        await cloudflareApi.UpdateRecordsIfNeeded(currentDnsRecords, recordsInThisZone, cloudflareConfig, ip, dryRun: cloudflareConfig.DryRun);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"Failed to update Zone {zoneId}. Error: {ex.Message}");
                        continue;
                    }
                }
                // try
                // {
                //     dnsRecords = await cloudflareApi.MakeDnsRecordModelFromResponse(await cloudflareApi.ListDnsRecords(cloudflareConfig.ZoneId, cloudflareConfig.ApiToken));

                // }
                // catch (System.Exception ex)
                // {
                //     Console.WriteLine($"Failed to list DNS records from cloudflare. error: {ex.Message}");
                //     Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
                //     await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
                //     continue;
                // }

                // try
                // {
                //     await cloudflareApi.UpdateRecordsIfNeeded(dnsRecords, cloudflareConfigRecords, cloudflareConfig, ip, dryRun: cloudflareConfig.DryRun);
                // }
                // catch (System.Exception ex)
                // {
                //     Console.WriteLine($"Failed to update DNS records. error: {ex.Message}");
                //     Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
                //     await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
                //     continue;
                // }

                lastIp = ip;
            }

            Console.WriteLine($"Waiting for {cloudflareConfig.IntervalMinutes} minutes before next check...");
            await Task.Delay(TimeSpan.FromMinutes(cloudflareConfig.IntervalMinutes));
        }
    }
}
