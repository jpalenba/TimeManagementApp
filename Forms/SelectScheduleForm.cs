#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TimeManagementApp.Forms
{
    /// <summary>
    /// Small dialog that lists every saved schedule snapshot and lets the
    /// user pick one to load.
    /// </summary>
    public partial class SelectScheduleForm : Form
    {
        private readonly ListBox listBoxSchedules = new();
        private readonly Button  btnOK            = new();
        private readonly Button  btnCancel        = new();

        private readonly List<ScheduleEntry> _entries;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScheduleEntry? SelectedEntry { get; private set; }

        public SelectScheduleForm(List<ScheduleEntry> entries)
        {
            _entries = entries ?? new List<ScheduleEntry>();
            InitializeComponent();
        }

        // ---------- UI ----------------------------------------------------
        private void InitializeComponent()
        {
            SuspendLayout();

            // listBoxSchedules
            listBoxSchedules.Dock   = DockStyle.Top;
            listBoxSchedules.Height = 220;
            foreach (var entry in _entries)
            {
                listBoxSchedules.Items.Add(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            // btnOK
            btnOK.Text   = "OK";
            btnOK.Dock   = DockStyle.Left;
            btnOK.Width  = 100;
            btnOK.Click += BtnOK_Click;

            // btnCancel
            btnCancel.Text   = "Cancel";
            btnCancel.Dock   = DockStyle.Right;
            btnCancel.Width  = 100;
            btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

            // form
            ClientSize = new Size(340, 260);
            Controls.Add(listBoxSchedules);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Text = "Select a Saved Schedule";

            ResumeLayout(false);
        }

        // ---------- events ------------------------------------------------
        private void BtnOK_Click(object? sender, EventArgs e)
        {
            int ix = listBoxSchedules.SelectedIndex;
            if (ix >= 0 && ix < _entries.Count)
            {
                SelectedEntry = _entries[ix];
                DialogResult  = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Please select a schedule first.");
            }
        }
    }

    // --------------------------------------------------------------------
    // Data-transfer object representing one saved weekly grid.
    // --------------------------------------------------------------------
    public class ScheduleEntry
    {
        /// <summary>When the user pressed “Save”.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Raw grid rows; each string[] = Time, Monday … Sunday.
        /// </summary>
        public List<string[]> ScheduleData { get; set; } = new();
    }
}
