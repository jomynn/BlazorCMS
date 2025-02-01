using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorCMS.Host.Services
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        public async Task<bool> PostAsync<T>(string url, T data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            return response.IsSuccessStatusCode;
        }
    }
}
