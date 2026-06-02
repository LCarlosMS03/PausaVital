using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PausaVital.Services
{
    public class ApiService
    {
        private const string BaseUrl = "http://127.0.0.1:8000";

        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(3)
        };

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
