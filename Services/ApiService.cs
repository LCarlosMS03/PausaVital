using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PausaVital.Services
{
    public class ApiService
    {
        private readonly HttpClient httpClient;
        private const string BaseUrl = "http://127.0.0.1:8000";

        public ApiService()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}