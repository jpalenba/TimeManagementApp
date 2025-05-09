using System;
using System.Drawing;
using System.Windows.Forms;
using TimeManagementApp;  // for BaseForm

namespace TimeManagementApp.Forms
{
    /// <summary>
    /// Popup dialog for editing a single calendar cell’s text.
    /// Inherits our shared theme from BaseForm.
    /// </summary>
    internal sealed class EventDialog : BaseForm
    {
        private readonly TextBox txt = new TextBox
        {
            Dock      = DockStyle.Fill,
            Multiline = true,
            Font      = new Font("Segoe UI", 10F, FontStyle.Regular),
            BackColor = Color.FromArgb(30, 30, 47),
            ForeColor = Color.White
        };

        /// <summary>What the user entered (trimmed).</summary>
        public string EventText => txt.Text.Trim();

        public EventDialog(string current)
        {
            Text       = "Edit Event";
            ClientSize = new Size(400, 200);

            // Text area
            Controls.Add(txt);
            txt.Text = current;

            // OK / Cancel panel
            var panel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(6),
                Height        = 40,
                BackColor     = this.BackColor
            };
            Controls.Add(panel);

            var btnOK = new Button
            {
                Text         = "OK",
                DialogResult = DialogResult.OK,
                Width        = 80,
                BackColor    = Color.FromArgb(31, 142, 241),
                ForeColor    = Color.White,
                Font         = this.Font
            };
            var btnCancel = new Button
            {
                Text         = "Cancel",
                DialogResult = DialogResult.Cancel,
                Width        = 80,
                BackColor    = Color.Gray,
                ForeColor    = Color.White,
                Font         = this.Font
            };

            panel.Controls.Add(btnOK);
            panel.Controls.Add(btnCancel);

            AcceptButton = btnOK;
            CancelButton = btnCancel;
        }
    }
}
