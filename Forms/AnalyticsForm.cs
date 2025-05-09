using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeManagementApp.Models;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public class AnalyticsForm : BaseForm
    {
        private readonly RichTextBox _output      = new() { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 10) };
        private readonly Button      _btnCompute  = new() { Text = "Compute Report",      Dock = DockStyle.Top,    Height = 30 };
        private readonly Button      _btnSuggest  = new() { Text = "Get AI Suggestions",  Dock = DockStyle.Top,    Height = 30, Enabled = false };
        private readonly TextBox     _txtFollowup = new() { Dock = DockStyle.Bottom, Height = 24, Enabled = false };
        private readonly Button      _btnAsk      = new() { Text = "Ask AI",              Dock = DockStyle.Bottom, Height = 30, Enabled = false };

        // Conversation history (role + content)
        private readonly List<(string Role, string Content)> _conversation = new();

        private static readonly string[] Categories = { "Work", "Study", "Personal", "Activity" };

        public AnalyticsForm()
        {
            Text       = "Schedule Analytics";
            ClientSize = new Size(700, 600);

            Controls.Add(_output);
            Controls.Add(_btnSuggest);
            Controls.Add(_btnCompute);
            Controls.Add(_txtFollowup);
            Controls.Add(_btnAsk);

            _btnCompute.Click += ComputeReport_Click;
            _btnSuggest.Click += async (_,__) => await GenerateAISuggestionsAsync();
            _btnAsk.Click     += async (_,__) => await SendFollowupAsync();
        }

        private void ComputeReport_Click(object sender, EventArgs e)
        {
            var tasks     = TaskRepository.Tasks;
            int total     = tasks.Count;
            int important = tasks.Count(t => t.IsImportant);
            int urgent    = tasks.Count(t => t.IsUrgent);
            int both      = tasks.Count(t => t.IsImportant && t.IsUrgent);

            var byCategory = Categories.ToDictionary(cat => cat, _ => 0);
            foreach (var t in tasks)
            {
                var cat = Categories.Contains(t.Category) ? t.Category : "Personal";
                byCategory[cat]++;
            }

            double totalHours = byCategory.Values.Sum(); // 1h per task

            var sb = new StringBuilder();
            sb.AppendLine($"Total tasks/events: {total}");
            sb.AppendLine($"Important: {important}");
            sb.AppendLine($"Urgent: {urgent}");
            sb.AppendLine($"Both important & urgent: {both}");
            sb.AppendLine();
            sb.AppendLine($"Estimated total hours this week: {totalHours}h");
            sb.AppendLine();
            sb.AppendLine("By category:");
            foreach (var cat in Categories)
                sb.AppendLine($"  • {cat}: {byCategory[cat]} ({byCategory[cat]}h)");

            _output.Text = sb.ToString();

            // Initialize conversation
            _conversation.Clear();
            _conversation.Add(("system", "You are a helpful productivity AI assistant."));
            _conversation.Add(("user", sb.ToString()));

            _btnSuggest.Enabled  = true;
            _txtFollowup.Enabled = false;
            _btnAsk.Enabled      = false;
        }

        private async Task GenerateAISuggestionsAsync()
        {
            _btnSuggest.Enabled = false;
            _output.AppendText("\n[AI] Generating suggestions...\n");

            // Add user instructions
            _conversation.Add(("user", 
                "Please analyze my weekly summary above and:\n" +
                "1. Identify any overloads or conflicts.\n" +
                "2. Recommend how to rebalance or reschedule tasks.\n" +
                "3. Offer two concrete time‑management tips based on their priority mix and categories."));

            // Build prompt string
            var prompt = new StringBuilder();
            foreach (var (role, content) in _conversation)
            {
                if (role == "system")
                    prompt.AppendLine($"[System]: {content}\n");
                else if (role == "user")
                    prompt.AppendLine($"[User]: {content}\n");
                else
                    prompt.AppendLine($"[Assistant]: {content}\n");
            }

            var response = await OpenAIService.ChatAsync(prompt.ToString());
            _conversation.Add(("assistant", response));

            _output.AppendText(response + "\n");

            // Enable follow‑up
            _txtFollowup.Enabled = true;
            _btnAsk.Enabled      = true;
        }

        private async Task SendFollowupAsync()
        {
            var question = _txtFollowup.Text.Trim();
            if (string.IsNullOrEmpty(question)) return;

            _output.AppendText($"\n[You]: {question}\n");
            _txtFollowup.Clear();
            _btnAsk.Enabled = false;

            _conversation.Add(("user", question));

            // Rebuild full prompt
            var prompt = new StringBuilder();
            foreach (var (role, content) in _conversation)
            {
                if (role == "system")
                    prompt.AppendLine($"[System]: {content}\n");
                else if (role == "user")
                    prompt.AppendLine($"[User]: {content}\n");
                else
                    prompt.AppendLine($"[Assistant]: {content}\n");
            }

            _output.AppendText("\n[AI] Thinking...\n");
            var reply = await OpenAIService.ChatAsync(prompt.ToString());
            _conversation.Add(("assistant", reply));

            _output.AppendText(reply + "\n");
            _btnAsk.Enabled = true;
        }
    }
}
