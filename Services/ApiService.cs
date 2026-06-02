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
            catch { return false; }
        }

        public async Task<int> LoginAsync(string username)
        {
            try
            {
                var payload = new { username = username };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await HttpClient.PostAsync("/auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("user_id").GetInt32();
                }
            }
            catch { }
            return 0;
        }

        public async Task<int> GetDefaultHabitAsync()
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync("/habits/default");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("habit_id").GetInt32();
                }
            }
            catch { }
            return 0;
        }

        public async Task<bool> RecordBreakAsync(int userId, int habitId, string status = "completed")
        {
            try
            {
                var payload = new { user_id = userId, habit_id = habitId, status = status };
                string json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpResponseMessage response = await HttpClient.PostAsync("/logs/", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<int> GetCurrentStreakAsync(int userId)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync($"/streaks/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("current_streak").GetInt32();
                }
            }
            catch { }
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
            catch { }
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
            catch { }
            return false;
        }

        public async Task<(int completed, int failed, double successRate)?> GetUserStatsAsync(int userId)
        {
            try
            {
                using HttpResponseMessage response = await HttpClient.GetAsync($"/stats/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);

                    int comp = doc.RootElement.GetProperty("total_completed").GetInt32();
                    int fail = doc.RootElement.GetProperty("total_failed").GetInt32();
                    double rate = doc.RootElement.GetProperty("success_rate").GetDouble();

                    return (comp, fail, rate);
                }
            }
            catch { }
            return null;
        }
    }
}