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

        public async Task<bool> RecordBreakAsync(int habitId, string status = "completed")
        {
            try
            {
                var payload = new { habit_id = habitId, status = status };
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
            }
            return 0;
        }

        public async Task<int> GetShieldsAsync(int userId)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync($"/shields/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("available_shields").GetInt32();
                }
            }
            catch
            {
            }
            return 0;
        }

        public async Task<bool> ConsumeShieldAsync(int userId)
        {
            try
            {
                var content = new StringContent("", Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await HttpClient.PostAsync($"/shields/{userId}/consume", content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("success").GetBoolean();
                }
            }
            catch
            {
            }
            return false;
        }
    }
}