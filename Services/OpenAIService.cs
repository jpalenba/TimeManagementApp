using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TimeManagementApp.Services
{
    public static class OpenAIService
    {
        /// <summary>
        /// Sends a free‐form prompt to OpenAI and returns only the assistant’s text.
        /// </summary>
        public static async Task<string> ChatAsync(string userPrompt)
        {
            var (content, headers) = await OpenAIHttpService.ChatCompletionAsync(
                "You are a helpful assistant.",
                userPrompt
            );
            return content;
        }

        /// <summary>
        /// Builds a time‐allocation prompt from hours‐per‐category,
        /// calls the HTTP helper, and returns just the AI’s reply text.
        /// </summary>
        public static async Task<string> SuggestTimeAnalyticsAsync(Dictionary<string, double> hoursPerCategory)
        {
            // Rebuild your prompt string here exactly as before:
            var lines = hoursPerCategory
                .OrderByDescending(kv => kv.Value)
                .Select(kv => $"- {kv.Key}: {kv.Value:F1}h");
            string prompt = "Here is how my time was allocated this week (in hours):\n"
                          + string.Join("\n", lines)
                          + "\n\nCan you suggest how I might reallocate my time to improve productivity, reduce burnout, or balance my schedule better?";

            var (content, headers) = await OpenAIHttpService.ChatCompletionAsync(
                "You are a helpful time‐management analyst.",
                prompt
            );
            return content;
        }
    }
}
