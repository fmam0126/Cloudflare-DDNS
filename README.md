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

### Run with .NET

1. Clone the repository
2. Copy `appsettings.EXAMPLE.json` to `appsettings.json`
3. Configure your settings in `appsettings.json`
4. Run the application:

```bash
dotnet run
```

### Run with Docker

A pre-built Docker image is available at `ghcr.io/fmam0126/cloudflare-ddns:main`.

#### Docker Compose (recommended)

```yaml
services:
  cloudflare-ddns:
    # Use the pre-built image from GitHub Container Registry
    image: ghcr.io/fmam0126/cloudflare-ddns:main
    # Alternatively, build locally:
    # build: .
    restart: unless-stopped
    env_file:
      - .env
```

1. Create a `.env` file in the project root (same level as `docker-compose.yml`)

2. Edit `.env` with your settings:

```env
CloudflareConfig__ApiToken=YOURTOKEN
CloudflareConfig__IntervalMinutes=5
CloudflareConfig__DryRun=false
CloudflareConfig__IpProvider=CloudflareTrace
DnsRecords=example.com,sub.example.com
```

3. Start the container:

```bash
docker compose up -d
```

#### Docker Run

```bash
docker run -d \
  --name cloudflare-ddns \
  --env-file .env \
  --restart unless-stopped \
  ghcr.io/fmam0126/cloudflare-ddns:main
```

#### Build Locally

To build and run the Docker image locally:

```bash
docker build -t cloudflare-ddns .
docker run -d --name cloudflare-ddns --env-file .env cloudflare-ddns
```

#### Configuration with Environment Variables

When running with Docker, configuration is done via environment variables instead of `appsettings.json`. The zone ID is automatically detected from your Cloudflare account — you only need to provide the API token and DNS record names. The following variables are supported:

| Environment Variable                            | Description                                      | Default                                                 |
| ----------------------------------------------- | ------------------------------------------------ | ------------------------------------------------------- |
| `CloudflareConfig__ApiToken`                    | Your Cloudflare API token                        | _required_                                              |
| `CloudflareConfig__IntervalMinutes`             | How often to check for IP changes                | `5`                                                     |
| `CloudflareConfig__DryRun`                      | Run without making actual DNS changes            | `false`                                                 |
| `CloudflareConfig__IpProvider`                  | IP detection provider to use                     | `CloudflareTrace`                                       |
| `CloudflareConfig__CloudflareTraceUrl`          | Custom URL for CloudflareTrace provider          | `https://one.one.one.one/cdn-cgi/trace`                 |
| `CloudflareConfig__CloudflareGeolocationApiUrl` | Custom URL for CloudflareGeolocationApi provider | `https://ipv4-check-perf.radar.cloudflare.com/api/info` |
| `CloudflareConfig__IpfyUrl`                     | Custom URL for Ipfy provider                     | `https://api.ipify.org`                                 |
| `CloudflareConfig__IcanhazipUrl`                | Custom URL for Icanhazip provider                | `https://ipv4.icanhazip.com`                            |
| `DnsRecords`                                    | Comma-separated list of A records to update      | _required_                                              |

**Supported `IpProvider` values:** `CloudflareTrace`, `CloudflareGeolocationApi`, `Ipfy`, `Icanhazip`

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
| IpProvider             | The IP provider to use for detecting your public IP          |
| CloudflareConfigRecord | List of DNS records to update                                |

### Getting Your Cloudflare Credentials

1. **API Token**: Go to Cloudflare Dashboard > Profile > API Tokens > Create Custom Token
   - Grant permissions: Zone:DNS:Edit, Zone:Zone:Read
2. **Zone ID**: Found in the Cloudflare Dashboard overview page for your domain

## How It Works

1. On startup, the application loads configuration from appsettings.json or environment variables
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
