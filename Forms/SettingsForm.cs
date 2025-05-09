#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public class SettingsForm : BaseForm
    {
        // holds app settings
        private class SettingsData
        {
            public bool   EnableNotifications { get; set; } = true;
            public string AppTheme           { get; set; } = "System Default";
        }

        private readonly string settingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private SettingsData settings = new();

        // UI tabs
        private readonly TabControl tabControl      = new();
        private readonly TabPage    tabSettings     = new();
        private readonly TabPage    tabHelp         = new();

        // settings controls
        private readonly CheckBox   chkNotifications = new();
        private readonly ComboBox   cmbTheme         = new();
        private readonly Button     btnApply         = new();

        // help text area
        private readonly RichTextBox txtHelp         = new();

        public SettingsForm()
        {
            Text       = "Settings & Help";
            ClientSize = new Size(600, 450);

            LoadSettingsFromFile();  // load existing settings
            InitializeComponent();   // build UI
            PopulateControls();      // set control values
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // main TabControl
            tabControl.Dock      = DockStyle.Fill;
            tabControl.Font      = Font;
            tabControl.BackColor = BackColor;
            tabControl.ForeColor = ForeColor;
            tabControl.TabPages.AddRange(new[] { tabSettings, tabHelp });
            Controls.Add(tabControl);

            // Settings tab layout
            tabSettings.Text      = "Settings";
            tabSettings.BackColor = BackColor;
            tabSettings.ForeColor = ForeColor;

            chkNotifications.Text     = "Enable desktop notifications";
            chkNotifications.AutoSize = true;
            chkNotifications.Font     = Font;
            chkNotifications.Top      = 20;
            chkNotifications.Left     = 20;
            tabSettings.Controls.Add(chkNotifications);

            var lblTheme = new Label {
                Text      = "App theme:",
                AutoSize  = true,
                Font      = Font,
                ForeColor = ForeColor,
                BackColor = BackColor,
                Top       = chkNotifications.Bottom + 20,
                Left      = 20
            };
            tabSettings.Controls.Add(lblTheme);

            cmbTheme.Font          = Font;
            cmbTheme.Top           = lblTheme.Bottom + 6;
            cmbTheme.Left          = 20;
            cmbTheme.Width         = 200;
            cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTheme.Items.AddRange(new[] { "Light", "Dark", "System Default" });
            tabSettings.Controls.Add(cmbTheme);

            btnApply.Text      = "Apply";
            btnApply.Font      = Font;
            btnApply.BackColor = ControlPaint.Light(BackColor);
            btnApply.ForeColor = ForeColor;
            btnApply.AutoSize  = true;
            btnApply.Top       = cmbTheme.Bottom + 30;
            btnApply.Left      = 20;
            btnApply.Click    += BtnApply_Click;  // save and apply
            tabSettings.Controls.Add(btnApply);

            // Help tab layout
            tabHelp.Text       = "Help";
            tabHelp.BackColor  = BackColor;
            tabHelp.ForeColor  = ForeColor;

            txtHelp.ReadOnly   = true;
            txtHelp.Dock       = DockStyle.Fill;
            txtHelp.Font       = new Font("Consolas", 10);
            txtHelp.BackColor  = BackColor;
            txtHelp.ForeColor  = ForeColor;
            txtHelp.Text =
@"Welcome to TimeManagementApp!

— Main Menu —
• Navigate between Calendar, Priorities, Analytics, and Settings using the sidebar.
• Resize or move the window using the title bar controls.

— Calendar —
1. When you open Calendar, choose:
   • Yes to load your saved schedule.
   • No to start with a blank calendar.
2. Double‑click any cell to add or edit an event:
   • Enter a description and select a category (Work, Study, Personal, Activity).
   • You can select multiple cells before confirming to add the same event to all.
3. Right‑click any cell to clear its contents.
4. Click 💾 Save (or press Ctrl+S) to save both your calendar and task data.
5. Click ⇩ Export CSV to download your week as a spreadsheet.

— Priorities —
• Drag and drop tasks between quadrants (Important/Urgent) to change their status.
• Right‑click any task title to toggle its Important or Urgent flag.
• Tasks sharing the same title are grouped together—moving or toggling one updates all duplicates.

— Analytics —
1. Click Compute Report to see:
   • Total tasks this week, grouped uniquely by title.
   • Counts of Important, Urgent, and both.
   • Total estimated hours (one hour per occurrence) and breakdown by category.
2. Get AI Suggestions for personalized rebalancing and time‑management tips.
3. Use the Ask AI box to follow up with any additional questions.

— Settings —
• Enable or disable desktop notifications.
• Choose between Light, Dark, or System Default theme.
• Click Apply to save and instantly update the app.

All your settings are saved automatically to **settings.json**. Enjoy staying organized!";
            tabHelp.Controls.Add(txtHelp);

            ResumeLayout(false);
        }

        private void PopulateControls()
        {
            chkNotifications.Checked = settings.EnableNotifications;
            cmbTheme.SelectedItem    = settings.AppTheme;
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            // update settings from UI
            settings.EnableNotifications = chkNotifications.Checked;
            settings.AppTheme            = cmbTheme.SelectedItem?.ToString() ?? "System Default";
            SaveSettingsToFile();

            // apply selected theme
            var theme = settings.AppTheme switch
            {
                "Light"           => ThemeService.Theme.Light,
                "Dark"            => ThemeService.Theme.Dark,
                _                 => ThemeService.Theme.System
            };
            ThemeService.ApplyTheme(theme);

            MessageBox.Show("Settings saved!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadSettingsFromFile()
        {
            // read settings.json if present
            try
            {
                if (File.Exists(settingsPath))
                {
                    var json = File.ReadAllText(settingsPath);
                    settings = JsonConvert.DeserializeObject<SettingsData>(json) ?? new SettingsData();
                }
            }
            catch
            {
                settings = new SettingsData();
            }
        }

        private void SaveSettingsToFile()
        {
            // write settings.json
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
