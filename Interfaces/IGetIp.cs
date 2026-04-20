namespace Cloudflare_DDNS.Interfaces;

public interface IGetIp
{
    Task<string> GetIpWithCloudflareTrace(string traceDomain);
    Task<string> GetIpWithCloudflareGeolocationApi(string geolocationUrl);
    Task<string> GetIpWithIpfy(string ipifyDomain);
    Task<string> GetIpWithicanhazip(string icanhazipDomain);
}