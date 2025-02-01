using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorCMS.Host.Services
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;   
        }

        public async Task<T> GetAsync<T>(string url)
        {
            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        public async Task<bool> PostAsync<T>(string url, T data)
        {
            _logger.LogDebug("Fetching data from {Url}", url);

            var response = await _httpClient.PostAsJsonAsync(url, data);

            return response.IsSuccessStatusCode;
        }
    }
}
