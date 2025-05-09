#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TimeManagementApp.Forms
{
    /// <summary>
    /// Dialog listing saved schedule snapshots for user selection.
    /// </summary>
    public partial class SelectScheduleForm : Form
    {
        // UI controls
        private readonly ListBox listBoxSchedules = new();  // shows timestamps
        private readonly Button  btnOK            = new();  // confirm selection
        private readonly Button  btnCancel        = new();  // cancel dialog

        private readonly List<ScheduleEntry> _entries;      // available snapshots

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScheduleEntry? SelectedEntry { get; private set; }  // chosen snapshot

        public SelectScheduleForm(List<ScheduleEntry> entries)
        {
            _entries = entries ?? new List<ScheduleEntry>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // populate list box with entry timestamps
            listBoxSchedules.Dock   = DockStyle.Top;
            listBoxSchedules.Height = 220;
            listBoxSchedules.Font   = Font;
            foreach (var entry in _entries)
                listBoxSchedules.Items.Add(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

            // OK button
            btnOK.Text   = "OK";
            btnOK.Dock   = DockStyle.Left;
            btnOK.Width  = 100;
            btnOK.Click += BtnOK_Click;

            // Cancel button
            btnCancel.Text   = "Cancel";
            btnCancel.Dock   = DockStyle.Right;
            btnCancel.Width  = 100;
            btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

            // form layout
            ClientSize = new Size(360, 260);
            Controls.Add(listBoxSchedules);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Text = "Select a Saved Schedule";

            ResumeLayout(false);
        }

        private void BtnOK_Click(object? sender, EventArgs e)
        {
            int ix = listBoxSchedules.SelectedIndex;
            if (ix >= 0 && ix < _entries.Count)
            {
                SelectedEntry = _entries[ix];     // set selection
                DialogResult  = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Please select a schedule first.");  // prompt if none
            }
        }
    }

    /// <summary>
    /// Represents a saved weekly schedule snapshot.
    /// </summary>
    public class ScheduleEntry
    {
        /// <summary>Timestamp when the user saved.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Grid data: each string[] has Time, Monday … Sunday.
        /// </summary>
        public List<string[]> ScheduleData { get; set; } = new();
    }
}
