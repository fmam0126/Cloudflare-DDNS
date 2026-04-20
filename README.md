# Cloudflare DDNS

A .NET console application that automatically updates Cloudflare DNS A records with your machine's public IP address.

## Features

- Automatically detects and updates your public IP in Cloudflare DNS records
- Supports multiple A records to update
- Configurable check interval
- Dry run mode for testing
- Multiple IP provider options
- Error handling for network failures 😳
- Configurable via JSON configuration file

## Requirements

- .NET 10.0 or later
- A Cloudflare account with an API token
- A domain configured in Cloudflare

## Installation

1. Clone the repository
2. Copy `appsettings.EXAMPLE.json` to `appsettings.json`
3. Configure your settings in `appsettings.json`
4. Run the application:

```bash
dotnet run
```

## Configuration

Edit `appsettings.json` with your Cloudflare credentials:

```json
{
  "CloudflareConfig": {
    "DryRun": false,
    "IntervalMinutes": 5,
    "ApiToken": "your_cloudflare_api_token_here",
    "ZoneId": "your_zone_id_here",
    "IpProvider": "CloudflareTrace"
  },
  "CloudflareConfigRecord": [
    {
      "name": "subdomain.example.com"
    },
    {
      "name": "example.com"
    }
  ]
}
```

### Configuration Options

| Setting                | Description                                                  |
| ---------------------- | ------------------------------------------------------------ |
| DryRun                 | When true, logs what would be updated without making changes |
| IntervalMinutes        | How often to check for IP changes                            |
| ApiToken               | Your Cloudflare API token                                    |
| ZoneId                 | The Cloudflare zone ID for your domain                       |
| IpProvider             | The IP provider to use for detecting your public IP          |
| CloudflareConfigRecord | List of DNS records to update                                |

### Getting Your Cloudflare Credentials

1. **API Token**: Go to Cloudflare Dashboard > Profile > API Tokens > Create Custom Token
   - Grant permissions: Zone:DNS:Edit, Zone:Zone:Read
2. **Zone ID**: Found in the Cloudflare Dashboard overview page for your domain

## How It Works

1. On startup, the application loads configuration from appsettings.json
2. It retrieves your current public IP using a configurable provider
3. It fetches the current DNS records from Cloudflare for your zone
4. For each configured A record, it compares the current IP with the DNS record
5. If the IP has changed, it updates the DNS record in Cloudflare
6. The process repeats indefinitely based on the configured interval

## IP Providers

The application supports multiple IP detection providers. You can configure which provider to use in the settings.

- **CloudflareTrace**: Uses Cloudflare's trace endpoint to get your IP
- **CloudflareGeolocationApi**: Uses Cloudflare's geolocation API to get your IP
- **Ipfy**: Uses the ipify service to get your IP
- **Icanhazip**: Uses the icanhazip service to get your IP

## Custom Urls for IP Providers

You can also specify custom URLs for the IP providers in the configuration file:

```json
{
  "CloudflareConfig": {
    "DryRun": false,
    "IntervalMinutes": 5,
    "ApiToken": "your_cloudflare_api_token_here",
    "ZoneId": "your_zone_id_here",
    "IpProvider": "CloudflareTrace",
    "CloudflareTraceUrl": "https://one.one.one.one/cdn-cgi/trace",
    "CloudflareGeolocationApiUrl": "https://ipv4-check-perf.radar.cloudflare.com/api/info",
    "IpfyUrl": "https://api.ipify.org",
    "IcanhazipUrl": "https://ipv4.icanhazip.com"
  }
}
```
