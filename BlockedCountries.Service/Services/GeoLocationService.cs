using BlockedCountries.BL.Dtos;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace BlockedCountries.BL.Services
{
    public class GeoLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        public GeoLocationService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
          
        }
        public async Task<IpLookupResultDto?> GetFullIpInfoAsync(string ip)
        {
            try
            {
                var url = $"{_config["GeoLocationApi:BaseUrl"]}?apiKey={_config["GeoLocationApi:ApiKey"]}&ip={ip}";
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json)!;

                return new IpLookupResultDto
                {
                    IpAddress = ip,
                    CountryCode = result.country_code2,
                    CountryName = result.country_name,
                    ISP = result.isp // or result.org_name depending on API
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetServerExternalIpAsync()
        {
            var response = await _httpClient.GetStringAsync("https://api.ipify.org");
            return response;
        }

    }
}
