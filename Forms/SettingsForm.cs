#nullable enable
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using TimeManagementApp;       // for BaseForm
using TimeManagementApp.Services;      // ← Make sure this is here


namespace TimeManagementApp.Forms
{
    /// <summary>
    /// User settings & help in one form, persisted to settings.json.
    /// Inherits shared styling from BaseForm.
    /// </summary>
    public class SettingsForm : BaseForm
    {
        private class SettingsData
        {
            public bool   EnableNotifications { get; set; } = true;
            public string AppTheme           { get; set; } = "System Default";
        }

        private readonly string settingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private SettingsData settings = new();

        private readonly TabControl tabControl      = new();
        private readonly TabPage    tabSettings     = new();
        private readonly TabPage    tabHelp         = new();

        private readonly CheckBox   chkNotifications = new();
        private readonly ComboBox   cmbTheme         = new();
        private readonly Button     btnApply         = new();

        private readonly RichTextBox txtHelp         = new();

        public SettingsForm()
        {
            Text       = "Settings & Help";
            ClientSize = new Size(600, 450);

            LoadSettingsFromFile();
            InitializeComponent();
            PopulateControls();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // TabControl
            tabControl.Dock      = DockStyle.Fill;
            tabControl.Font      = this.Font;
            tabControl.BackColor = this.BackColor;
            tabControl.ForeColor = this.ForeColor;
            tabControl.TabPages.AddRange(new[] { tabSettings, tabHelp });
            Controls.Add(tabControl);

            // --- Settings Tab ---
            tabSettings.Text      = "Settings";
            tabSettings.BackColor = this.BackColor;
            tabSettings.ForeColor = this.ForeColor;

            // Notifications checkbox
            chkNotifications.Text     = "Enable desktop notifications";
            chkNotifications.AutoSize = true;
            chkNotifications.Font     = this.Font;
            chkNotifications.Top      = 20;
            chkNotifications.Left     = 20;
            tabSettings.Controls.Add(chkNotifications);

            // Theme label
            var lblTheme = new Label {
                Text      = "App theme:",
                AutoSize  = true,
                Font      = this.Font,
                ForeColor = this.ForeColor,
                BackColor = this.BackColor,
                Top       = chkNotifications.Bottom + 20,
                Left      = 20
            };
            tabSettings.Controls.Add(lblTheme);

            // Theme combo
            cmbTheme.Font          = this.Font;
            cmbTheme.Top           = lblTheme.Bottom + 6;
            cmbTheme.Left          = 20;
            cmbTheme.Width         = 200;
            cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTheme.Items.AddRange(new[] { "Light", "Dark", "System Default" });
            tabSettings.Controls.Add(cmbTheme);

            // Apply button
            btnApply.Text      = "Apply";
            btnApply.Font      = this.Font;
            btnApply.BackColor = ControlPaint.Light(this.BackColor);
            btnApply.ForeColor = this.ForeColor;
            btnApply.AutoSize  = true;
            btnApply.Top       = cmbTheme.Bottom + 30;
            btnApply.Left      = 20;
            btnApply.Click    += BtnApply_Click;
            tabSettings.Controls.Add(btnApply);

            // --- Help Tab ---
            tabHelp.Text       = "Help";
            tabHelp.BackColor  = this.BackColor;
            tabHelp.ForeColor  = this.ForeColor;

            txtHelp.ReadOnly   = true;
            txtHelp.Dock       = DockStyle.Fill;
            txtHelp.Font       = this.Font;
            txtHelp.BackColor  = this.BackColor;
            txtHelp.ForeColor  = this.ForeColor;
            txtHelp.Text       =
@"TimeManagementApp Help

– To add an event: double‑click any cell in the calendar.
– Save your schedule any time with the 💾 button or Ctrl+S.
– Export your week to CSV via ⇩ Export CSV.
– In Settings you can toggle notifications and pick a theme.
– Your choices are saved automatically to settings.json.";
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
        // ... save to settings.json ...
        SaveSettingsToFile();

        // immediately apply new theme
        var sel = cmbTheme.SelectedItem?.ToString() ?? "System Default";
        var theme = sel switch
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
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings:\n{ex.Message}",
                                Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
