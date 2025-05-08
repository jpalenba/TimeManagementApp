using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public class AnalyticsForm : Form
    {
        private Button btnCompute  = new Button { Text = "Compute Report",  Dock = DockStyle.Top,    Height = 30 };
        private Button btnSuggest  = new Button { Text = "Get Suggestions", Dock = DockStyle.Top,    Height = 30, Enabled = false };
        private RichTextBox output = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10), ReadOnly = true };

        // holds the last computed report
        private Dictionary<string,double> _lastReport = new Dictionary<string,double>();

        public AnalyticsForm()
        {
            Text       = "Time Analytics";
            ClientSize = new Size(600, 500);

            Controls.Add(output);
            Controls.Add(btnSuggest);
            Controls.Add(btnCompute);

            btnCompute.Click += ComputeReport_Click;
            btnSuggest.Click += async (_,__) =>
            {
                btnSuggest.Enabled = false;
                var suggestions = await OpenAIService.SuggestTimeAnalyticsAsync(_lastReport);
                output.AppendText("\n\n=== AI Suggestions ===\n" + suggestions);
                btnSuggest.Enabled = true;
            };
        }

        private void ComputeReport_Click(object sender, EventArgs e)
        {
            // 1) Gather hours-per-category from your stored tasks
            var hours = new Dictionary<string,double>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in TaskRepository.Tasks)
            {
                // category = text before colon, or “Uncategorized”
                var cat = t.Title?.Split(':').FirstOrDefault()?.Trim() ?? "Uncategorized";
                hours[cat] = hours.GetValueOrDefault(cat) + 1.0;
            }

            _lastReport = hours;

            // 2) Display the breakdown
            output.Clear();
            output.AppendText("=== Time Spent by Category ===\n");
            foreach (var kv in hours.OrderByDescending(kv=>kv.Value))
                output.AppendText($"{kv.Key.PadRight(15)} : {kv.Value:F1} h\n");

            btnSuggest.Enabled = hours.Count > 0;
        }
    }
}
