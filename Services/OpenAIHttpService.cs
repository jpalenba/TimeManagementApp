using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TimeManagementApp.Services
{
    public static class OpenAIHttpService
    {
        // RNG for back‑off jitter
        private static readonly Random _rng = new Random();

        // load API key from environment
        private static readonly string _apiKey =
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("Set OPENAI_API_KEY.");

        // shared HTTP client
        private static readonly HttpClient _client = new HttpClient();

        static OpenAIHttpService()
        {
            // set bearer token
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// Sends a chat completion request with retry on rate‑limit.
        /// </summary>
        public static async Task<(string Content, HttpResponseHeaders Headers)> ChatCompletionAsync(
            string systemPrompt,
            string userPrompt)
        {
            const int maxRetries = 5;
            int delayMs = 500;

            // prepare request body
            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt }
                }
            };
            string jsonBody = JsonConvert.SerializeObject(payload);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var content  = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                using var response = await _client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                string raw = await response.Content.ReadAsStringAsync();

                // parse error if any
                dynamic errJson = null;
                if (!response.IsSuccessStatusCode)
                {
                    try { errJson = JsonConvert.DeserializeObject(raw); } catch { }
                }

                // bail on insufficient quota
                if (response.StatusCode == (HttpStatusCode)429
                    && errJson?.error?.code == "insufficient_quota")
                {
                    return ($"[Quota Error]: {(string)errJson.error.message}", response.Headers);
                }

                // retry on rate‑limit
                if (response.StatusCode == (HttpStatusCode)429 && attempt < maxRetries)
                {
                    int jitter = _rng.Next(-100, 101);
                    await Task.Delay(delayMs + jitter);
                    delayMs = Math.Min(delayMs * 2, 5000);
                    continue;
                }

                // return other errors
                if (!response.IsSuccessStatusCode)
                {
                    string code = errJson?.error?.code ?? ((int)response.StatusCode).ToString();
                    string msg  = errJson?.error?.message ?? raw;
                    return ($"[Error {code}]: {msg}", response.Headers);
                }

                // successful response
                dynamic result = JsonConvert.DeserializeObject(raw)!;
                string answer  = (string)result.choices[0].message.content;
                return (answer, response.Headers);
            }

            // unreachable
            return ("[Error]: Exceeded retries", new HttpResponseMessage().Headers);
        }

        /// <summary>
        /// Builds a time‑allocation prompt and calls ChatCompletionAsync.
        /// </summary>
        public static Task<(string Content, HttpResponseHeaders Headers)> SuggestTimeAnalyticsAsync(
            Dictionary<string, double> hoursPerCategory)
        {
            var lines = hoursPerCategory
                .OrderByDescending(kv => kv.Value)
                .Select(kv => $"- {kv.Key}: {kv.Value:F1}h");
            string prompt =
                "Here is my weekly time allocation (hours):\n"
                + string.Join("\n", lines)
                + "\n\nHow can I rebalance to boost productivity and prevent burnout?";

            return ChatCompletionAsync("You are a time‑management analyst.", prompt);
        }
    }
}
