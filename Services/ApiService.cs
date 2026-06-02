using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

        public async Task<bool> RecordBreakAsync(int habitId)
        {
            try
            {
                var payload = new { habit_id = habitId, status = "completed" };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await HttpClient.PostAsync("/logs/", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync("/streaks/");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("current_streak").GetInt32();
                }
            }
            catch
            {
                // Return 0 if the backend is unreachable
            }
            return 0;
        }
    }
}