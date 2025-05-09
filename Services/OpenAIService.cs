using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeManagementApp.Services
{
    public static class OpenAIService
    {
        // Send a free‑form user prompt and return only the assistant’s response text
        public static async Task<string> ChatAsync(string userPrompt)
        {
            var (content, headers) = await OpenAIHttpService.ChatCompletionAsync(
                "You are a helpful assistant.",
                userPrompt
            );
            return content;
        }

        // Build a time‑allocation prompt from category hours and return the AI’s suggestions
        public static async Task<string> SuggestTimeAnalyticsAsync(Dictionary<string, double> hoursPerCategory)
        {
            var lines = hoursPerCategory
                .OrderByDescending(kv => kv.Value)
                .Select(kv => $"- {kv.Key}: {kv.Value:F1}h");
            string prompt = "Here is how my time was allocated this week (in hours):\n"
                          + string.Join("\n", lines)
                          + "\n\nCan you suggest how I might reallocate my time to improve productivity, reduce burnout, or balance my schedule better?";

            var (content, headers) = await OpenAIHttpService.ChatCompletionAsync(
                "You are a helpful time‑management analyst.",
                prompt
            );
            return content;
        }
    }
}
