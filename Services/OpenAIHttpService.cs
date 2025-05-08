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
        // ← NEW: used for jitter in back‑off
        private static readonly Random _rng = new Random();

        private static readonly string _apiKey =
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? throw new InvalidOperationException("Please set the OPENAI_API_KEY environment variable.");

        private static readonly HttpClient _client = new HttpClient();

        static OpenAIHttpService()
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// Sends a ChatCompletion request to OpenAI (gpt-3.5-turbo).
        /// Retries up to 5 times on HTTP 429 (Too Many Requests) using exponential back‑off + jitter.
        /// Returns the assistant’s reply and the response headers.
        /// </summary>
        public static async Task<(string Content, HttpResponseHeaders Headers)> ChatCompletionAsync(
    string systemPrompt,
    string userPrompt)
{
    const int maxRetries = 5;
    int delayMs          = 500;
    var jitterRng        = new Random();

    // Build payload once
    var payload = new
    {
        model    = "gpt-3.5-turbo",
        messages = new[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user",   content = userPrompt  }
        }
    };
    string jsonBody = JsonConvert.SerializeObject(payload);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            content
        );

        string raw = await response.Content.ReadAsStringAsync();

        // Parse error JSON if we failed
        dynamic errJson = null;
        if (!response.IsSuccessStatusCode)
        {
            try { errJson = JsonConvert.DeserializeObject(raw); }
            catch { /* ignore */ }
        }

        // 1) If we’ve hit a real “insufficient_quota”, bail immediately
        if (response.StatusCode == (HttpStatusCode)429
         && errJson?.error?.code == "insufficient_quota")
        {
            string msg = (string)errJson.error.message;
            return ($"[Quota Error]: {msg}", response.Headers);
        }

        // 2) If it’s a **rate‑limit** 429, retry with back‑off+jitter (unless last attempt)
        if (response.StatusCode == (HttpStatusCode)429 && attempt < maxRetries)
        {
            int jitter = jitterRng.Next(-100, 101);
            await Task.Delay(delayMs + jitter);
            delayMs = Math.Min(delayMs * 2, 5000);
            continue;
        }

        // 3) If any other HTTP error (or final 429), return its message once
        if (!response.IsSuccessStatusCode)
        {
            string code = errJson?.error?.code ?? ((int)response.StatusCode).ToString();
            string msg  = errJson?.error?.message ?? raw;
            return ($"[Error {code}]: {msg}", response.Headers);
        }

        // 4) Success path
        dynamic result = JsonConvert.DeserializeObject(raw)!;
        string answer = (string)result.choices[0].message.content;
        return (answer, response.Headers);
    }

    // Should never reach here
    return ("[Error]: Exceeded max retry attempts", new HttpResponseMessage().Headers);
}

        /// <summary>
        /// Builds your hours‑per‑category prompt and calls ChatCompletionAsync.
        /// </summary>
        public static Task<(string Content, HttpResponseHeaders Headers)> SuggestTimeAnalyticsAsync(
            Dictionary<string, double> hoursPerCategory)
        {
            var lines = hoursPerCategory
                .OrderByDescending(kv => kv.Value)
                .Select(kv => $"- {kv.Key}: {kv.Value:F1}h");
            string prompt = "Here is how my time was allocated this week (in hours):\n"
                          + string.Join("\n", lines)
                          + "\n\nCan you suggest how I might reallocate my time to improve productivity, reduce burnout, or balance my schedule better?";

            return ChatCompletionAsync(
                "You are a helpful time-management analyst.",
                prompt
            );
        }
    }
}
