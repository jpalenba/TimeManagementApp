using System;
using System.Drawing;
using System.Windows.Forms;
using TimeManagementApp.Services;

namespace TimeManagementApp
{
    public class BaseForm : Form
    {
        public BaseForm()
        {
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // Subscribe early so dynamic changes work
            ThemeService.ThemeChanged += (_,_) => ApplyCurrentTheme();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Ensure the current theme is applied after all child controls exist
            ApplyCurrentTheme();
        }

        private void ApplyCurrentTheme()
        {
            var (bg, fg) = ThemeService.GetColors();

            // Form itself
            BackColor = bg;
            ForeColor = fg;

            // Recursively style every control
            ApplyToControls(Controls, bg, fg);
        }

        private void ApplyToControls(Control.ControlCollection ctrls, Color bg, Color fg)
        {
            foreach (Control ctl in ctrls)
            {
                ctl.BackColor = bg;
                ctl.ForeColor = fg;

                // Special cases you might want to tweak:
                if (ctl is DataGridView dgv)
                {
                    dgv.BackgroundColor               = bg;
                    dgv.DefaultCellStyle.BackColor    = bg;
                    dgv.DefaultCellStyle.ForeColor    = fg;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = ControlPaint.Dark(bg);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = fg;
                    dgv.GridColor = ControlPaint.Dark(bg);
                }
                else if (ctl is Button btn)
                {
                    // keep buttons legible
                    btn.BackColor = ControlPaint.Light(bg);
                    btn.ForeColor = fg;
                }

                // Recurse into children
                if (ctl.HasChildren)
                    ApplyToControls(ctl.Controls, bg, fg);
            }
        }
    }
}
