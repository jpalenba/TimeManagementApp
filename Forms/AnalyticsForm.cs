using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public class AnalyticsForm : BaseForm
    {
        // UI elements
        private readonly RichTextBox _output = new() { 
            Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 10) 
        };
        private readonly Button _btnCompute  = new() { 
            Text = "Compute Report", Dock = DockStyle.Top,    Height = 30 
        };
        private readonly Button _btnSuggest  = new() { 
            Text = "Get AI Suggestions", Dock = DockStyle.Top,    Height = 30, Enabled = false 
        };
        private readonly TextBox _txtFollowup = new() { 
            Dock = DockStyle.Bottom, Height = 24, Enabled = false 
        };
        private readonly Button _btnAsk  = new() { 
            Text = "Ask AI", Dock = DockStyle.Bottom, Height = 30, Enabled = false 
        };

        // conversation history for AI
        private readonly List<(string Role, string Content)> _conversation = new();

        // task categories
        private static readonly string[] Categories = { "Work", "Study", "Personal", "Activity" };

        public AnalyticsForm()
        {
            Text       = "Schedule Analytics";   
            ClientSize = new Size(700, 600);     

            // add controls in z-order
            Controls.Add(_output);
            Controls.Add(_btnSuggest);
            Controls.Add(_btnCompute);
            Controls.Add(_txtFollowup);
            Controls.Add(_btnAsk);

            // event handlers
            _btnCompute.Click += ComputeReport_Click;
            _btnSuggest.Click += async (_,__) => await GenerateAISuggestionsAsync();
            _btnAsk.Click     += async (_,__) => await SendFollowupAsync();
        }

        private void ComputeReport_Click(object sender, EventArgs e)
        {
            // load tasks
            var allTasks = TaskRepository.Tasks;
            // dedupe by title
            var unique   = allTasks.GroupBy(t => t.Title).Select(g => g.First()).ToList();

            // basic metrics
            int totalUnique = unique.Count;
            int impUnique   = unique.Count(t => t.IsImportant);
            int urgUnique   = unique.Count(t => t.IsUrgent);
            int bothUnique  = unique.Count(t => t.IsImportant && t.IsUrgent);
            int totalHours  = allTasks.Count; 

            // count hours per category
            var hoursByCatAll = Categories.ToDictionary(cat => cat, _ => 0);
            foreach (var t in allTasks)
            {
                var cat = Categories.Contains(t.Category) ? t.Category : "Personal";
                hoursByCatAll[cat]++;
            }

            // count unique tasks per category
            var countByCatUnique = Categories.ToDictionary(cat => cat, _ => 0);
            foreach (var t in unique)
            {
                var cat = Categories.Contains(t.Category) ? t.Category : "Personal";
                countByCatUnique[cat]++;
            }

            // build report text
            var sb = new StringBuilder();
            sb.AppendLine($"Total unique tasks/events: {totalUnique}");
            sb.AppendLine($"Important (unique): {impUnique}");
            sb.AppendLine($"Urgent (unique): {urgUnique}");
            sb.AppendLine($"Both important & urgent (unique): {bothUnique}");
            sb.AppendLine();
            sb.AppendLine($"Estimated total hours this week: {totalHours}h");
            sb.AppendLine();
            sb.AppendLine("By category (unique count / hours):");
            foreach (var cat in Categories)
                sb.AppendLine($"  • {cat}: {countByCatUnique[cat]} tasks, {hoursByCatAll[cat]}h");

            _output.Text = sb.ToString();

            // prepare AI context
            _conversation.Clear();
            _conversation.Add(("system", "You are a helpful productivity AI assistant."));
            _conversation.Add(("user", sb.ToString()));

            // enable next steps
            _btnSuggest.Enabled  = true;
            _txtFollowup.Enabled = false;
            _btnAsk.Enabled      = false;
        }

        private async Task GenerateAISuggestionsAsync()
        {
            _btnSuggest.Enabled = false;
            _output.AppendText("\n[AI] Generating suggestions...\n");

            // add user request to conversation
            _conversation.Add(("user",
                "Please analyze my weekly summary and:\n" +
                "1. Identify overloads or conflicts.\n" +
                "2. Recommend how to rebalance tasks.\n" +
                "3. Offer two concrete time‑management tips."));

            // format prompt
            var prompt = new StringBuilder();
            foreach (var (role, content) in _conversation)
                prompt.AppendLine($"{(role == "system" ? "[System]" : "[User]")}: {content}\n");

            var response = await OpenAIService.ChatAsync(prompt.ToString());
            _conversation.Add(("assistant", response));

            _output.AppendText(response + "\n");
            _txtFollowup.Enabled = true;
            _btnAsk.Enabled      = true;
        }

        private async Task SendFollowupAsync()
        {
            var question = _txtFollowup.Text.Trim();
            if (question == "") return;    

            _output.AppendText($"\n[You]: {question}\n");
            _txtFollowup.Clear();
            _btnAsk.Enabled = false;

            _conversation.Add(("user", question));

            // rebuild prompt
            var prompt = new StringBuilder();
            foreach (var (role, content) in _conversation)
                prompt.AppendLine($"{(role == "system" ? "[System]" : "[User]")}: {content}\n");

            _output.AppendText("\n[AI] Thinking...\n");
            var reply = await OpenAIService.ChatAsync(prompt.ToString());
            _conversation.Add(("assistant", reply));

            _output.AppendText(reply + "\n");
            _btnAsk.Enabled = true;
        }
    }
}
