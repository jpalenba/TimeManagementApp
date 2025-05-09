#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using TimeManagementApp.Models;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public partial class CalendarForm : BaseForm
    {
        private static readonly string[] Days =
            { "Time", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        private static readonly string[] Categories = { "Work", "Study", "Personal", "Activity" };

        private static readonly Dictionary<string, Color> CategoryColours = new()
        {
            ["Work"]     = Color.FromArgb(0,   120, 215),
            ["Study"]    = Color.FromArgb(0,   153,  51),
            ["Personal"] = Color.FromArgb(204,   0,   0),
            ["Activity"] = Color.FromArgb(102,   0, 204)
        };

        private readonly DataGridView scheduleGrid = new();
        private readonly Panel       buttonPanel  = new();
        private readonly Button      btnSave      = new();
        private readonly Button      btnExport    = new();
        private readonly ContextMenuStrip cellMenu = new();

        public CalendarForm()
        {
            KeyPreview = true;
            InitializeComponent();

            // Apply theme
            scheduleGrid.Font                       = Font;
            scheduleGrid.BackgroundColor            = BackColor;
            scheduleGrid.DefaultCellStyle.BackColor = BackColor;
            scheduleGrid.DefaultCellStyle.ForeColor = ForeColor;
            scheduleGrid.GridColor                  = ControlPaint.Dark(BackColor);
            var hdr = scheduleGrid.ColumnHeadersDefaultCellStyle;
            hdr.BackColor = ControlPaint.Dark(BackColor);
            hdr.ForeColor = ForeColor;
            hdr.Font      = new Font(Font, FontStyle.Bold);

            // Prompt to load or create
            var dlg = MessageBox.Show(
                "Do you want to load an existing schedule?\n\nYes = Load file\nNo = Create new blank",
                "Schedule",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (dlg == DialogResult.Yes && File.Exists("schedule.json"))
                LoadSchedule();
            else
            {
                InitializeScheduleGrid();
                TaskRepository.Tasks.Clear();
                TaskRepository.Save();
            }
        }

        private void InitializeComponent()
        {
            Text        = "Weekly Schedule";
            ClientSize  = new Size(880, 720);
            MinimumSize = new Size(640, 480);

            FormClosing += CalendarForm_FormClosing;
            KeyDown     += CalendarForm_KeyDown;
            Resize      += CalendarForm_Resize;

            // Button panel
            buttonPanel.Dock      = DockStyle.Bottom;
            buttonPanel.Height    = 45;
            buttonPanel.Padding   = new Padding(8);
            buttonPanel.BackColor = BackColor;
            Controls.Add(buttonPanel);

            btnSave.Text   = "💾 Save (Ctrl+S)";
            btnSave.Dock   = DockStyle.Right;
            btnSave.Width  = 140;
            btnSave.Click += (_,_) => SaveSchedule();
            btnSave.BackColor = ControlPaint.Light(BackColor);
            btnSave.ForeColor = ForeColor;
            buttonPanel.Controls.Add(btnSave);

            btnExport.Text   = "⇩ Export CSV";
            btnExport.Dock   = DockStyle.Right;
            btnExport.Width  = 120;
            btnExport.Margin = new Padding(0,0,6,0);
            btnExport.Click += BtnExport_Click;
            btnExport.BackColor = ControlPaint.Light(BackColor);
            btnExport.ForeColor = ForeColor;
            buttonPanel.Controls.Add(btnExport);

            // Schedule grid
            scheduleGrid.Dock              = DockStyle.Fill;
            scheduleGrid.ReadOnly          = true;
            scheduleGrid.RowHeadersVisible = false;
            scheduleGrid.AllowUserToAddRows    = false;
            scheduleGrid.AllowUserToDeleteRows = false;
            scheduleGrid.AllowUserToResizeRows = false;
            scheduleGrid.SelectionMode     = DataGridViewSelectionMode.CellSelect;
            scheduleGrid.MultiSelect       = true;
            scheduleGrid.CellDoubleClick       += ScheduleGrid_CellDoubleClick;
            scheduleGrid.CellMouseClick        += ScheduleGrid_CellMouseClick;
            scheduleGrid.CellToolTipTextNeeded += ScheduleGrid_CellToolTipTextNeeded;
            scheduleGrid.CellFormatting        += ScheduleGrid_CellFormatting;
            Controls.Add(scheduleGrid);

            // Context menu
            cellMenu.Items.Add("Clear", null, (_,_) =>
            {
                foreach (DataGridViewCell c in scheduleGrid.SelectedCells)
                    if (c.ColumnIndex > 0) c.Value = string.Empty;
            });
        }

        private void InitializeScheduleGrid()
        {
            scheduleGrid.Columns.Clear();
            scheduleGrid.Rows.Clear();

            foreach (var d in Days)
                scheduleGrid.Columns.Add(d, d);

            scheduleGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            scheduleGrid.Columns[0].Width        = 70;
            for (int i = 1; i < scheduleGrid.Columns.Count; i++)
                scheduleGrid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            var dt  = DateTime.Today.AddHours(6);
            var end = DateTime.Today.AddDays(1);
            while (dt <= end)
            {
                int idx = scheduleGrid.Rows.Add();
                scheduleGrid.Rows[idx].Cells[0].Value =
                    dt.ToString("h:mm tt", CultureInfo.InvariantCulture);
                dt = dt.AddHours(1);
            }

            AdjustRowHeights();
        }

        private void LoadSchedule()
        {
            var data = JsonConvert
                .DeserializeObject<List<string[]>>(File.ReadAllText("schedule.json"))
                ?? new List<string[]>();
            InitializeScheduleGrid();
            for (int r = 0; r < data.Count && r < scheduleGrid.Rows.Count; r++)
                for (int c = 1; c < data[r].Length && c < scheduleGrid.Columns.Count; c++)
                    scheduleGrid.Rows[r].Cells[c].Value = data[r][c];
            AdjustRowHeights();

            TaskRepository.Load();
        }

        private void SaveSchedule()
        {
            // save grid to schedule.json
            var rows = scheduleGrid.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => r.Cells
                    .Cast<DataGridViewCell>()
                    .Select(c => c.Value?.ToString() ?? "")
                    .ToArray())
                .ToList();
            File.WriteAllText("schedule.json",
                JsonConvert.SerializeObject(rows, Formatting.Indented));

            // overwrite tasks.json from grid contents
            TaskRepository.Tasks.Clear();
            for (int r = 0; r < scheduleGrid.Rows.Count; r++)
            {
                for (int c = 1; c < scheduleGrid.Columns.Count; c++)
                {
                    var cell = scheduleGrid.Rows[r].Cells[c].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(cell)) continue;
                    var parts = cell.Split(new[] { ": " }, 2, StringSplitOptions.None);
                    var cat   = parts[0];
                    var title = parts.Length > 1 ? parts[1] : "";
                    if (!Categories.Contains(cat)) cat = "Personal";
                    if (DateTime.TryParseExact(
                        scheduleGrid.Rows[r].Cells[0].Value?.ToString(),
                        "h:mm tt", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var dt))
                    {
                        TaskRepository.Tasks.Add(new CalendarTask
                        {
                            Day         = scheduleGrid.Columns[c].HeaderText,
                            Time        = dt.TimeOfDay,
                            Title       = title,
                            Category    = cat,
                            IsImportant = false,
                            IsUrgent    = false
                        });
                    }
                }
            }
            TaskRepository.Save();

            MessageBox.Show("Schedule and tasks saved!", "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter   = "CSV files|*.csv",
                FileName = "schedule.csv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            using var sw = new StreamWriter(dlg.FileName);
            sw.WriteLine(string.Join(',', Days));
            foreach (DataGridViewRow row in scheduleGrid.Rows)
            {
                if (row.IsNewRow) continue;
                var vals = row.Cells.Cast<DataGridViewCell>()
                                   .Select(c => (c.Value?.ToString() ?? "").Replace(',', ';'));
                sw.WriteLine(string.Join(',', vals));
            }
        }

        private void ScheduleGrid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;

            string day      = scheduleGrid.Columns[e.ColumnIndex].HeaderText;
            string timeText = scheduleGrid.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";

            using var dlg = new Form
            {
                Text            = "New Event",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition   = FormStartPosition.CenterParent,
                ClientSize      = new Size(360, 200),
                MinimizeBox     = false,
                MaximizeBox     = false
            };

            var lblDesc = new Label { Text = $"Event for {day} at {timeText}:", AutoSize = true, Top = 15, Left = 10 };
            var txtDesc = new TextBox { Top = lblDesc.Bottom + 5, Left = 10, Width = 340 };
            var existing = scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
            if (existing.Contains(": "))
                txtDesc.Text = existing.Split(new[] { ": " }, 2, StringSplitOptions.None)[1];
            else
                txtDesc.Text = existing;

            var lblCat = new Label { Text = "Category:", AutoSize = true, Top = txtDesc.Bottom + 15, Left = 10 };
            var cmbCat = new ComboBox
            {
                Top           = lblCat.Bottom + 5,
                Left          = 10,
                Width         = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCat.Items.AddRange(Categories);
            var exCat = existing.Contains(": ")
                        ? existing.Split(new[] { ": " }, 2, StringSplitOptions.None)[0]
                        : Categories[0];
            cmbCat.SelectedItem = Categories.Contains(exCat) ? exCat : Categories[0];

            var btnOk     = new Button { Text = "OK",     DialogResult = DialogResult.OK, Top = cmbCat.Bottom + 20, Left = 190, Width = 75 };
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Top = cmbCat.Bottom + 20, Left = 275, Width = 75 };

            dlg.Controls.AddRange(new Control[] { lblDesc, txtDesc, lblCat, cmbCat, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string desc = txtDesc.Text.Trim();
                string cat  = cmbCat.SelectedItem?.ToString() ?? Categories[0];
                string cellText = string.IsNullOrWhiteSpace(desc) ? "" : $"{cat}: {desc}";

                foreach (DataGridViewCell cell in scheduleGrid.SelectedCells)
                {
                    if (cell.RowIndex >= 0 && cell.ColumnIndex > 0)
                    {
                        scheduleGrid.Rows[cell.RowIndex].Cells[cell.ColumnIndex].Value = cellText;
                        UpsertTask(cell.RowIndex, cell.ColumnIndex, desc, cat);
                    }
                }
            }
        }

        private void ScheduleGrid_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            scheduleGrid.CurrentCell = scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            cellMenu.Show(Cursor.Position);
        }

        private void ScheduleGrid_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            var txt = scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
            if (!string.IsNullOrWhiteSpace(txt))
                e.ToolTipText = txt;
        }

        private void ScheduleGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            if (e.Value is not string t || string.IsNullOrWhiteSpace(t)) return;

            var idx = t.IndexOf(':');
            if (idx <= 0) return;
            var cat = t.Substring(0, idx).Trim();
            if (CategoryColours.TryGetValue(cat, out var col))
                e.CellStyle.BackColor = col;
        }

        private void CalendarForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            var dr = MessageBox.Show("Save changes before closing?", Text,
                                     MessageBoxButtons.YesNoCancel,
                                     MessageBoxIcon.Question);
            if (dr == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (dr == DialogResult.Yes)
            {
                SaveSchedule();
            }
        }

        private void CalendarForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                SaveSchedule();
            }
        }

        private void CalendarForm_Resize(object? sender, EventArgs e)
        {
            AdjustRowHeights();
        }

        private void AdjustRowHeights()
        {
            int count = scheduleGrid.Rows.Count;
            if (count == 0) return;

            int avail = ClientSize.Height
                      - buttonPanel.Height
                      - scheduleGrid.ColumnHeadersHeight;
            if (avail <= 0) return;

            int baseH = avail / count;
            int extra = avail - baseH * count;
            for (int i = 0; i < count; i++)
                scheduleGrid.Rows[i].Height = baseH + (i < extra ? 1 : 0);
        }

        private void UpsertTask(int row, int col, string title, string category)
        {
            string day       = scheduleGrid.Columns[col].HeaderText;
            string timeLabel = scheduleGrid.Rows[row].Cells[0].Value?.ToString() ?? "";
            if (!DateTime.TryParseExact(
                timeLabel, "h:mm tt",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
                return;

            var task = new CalendarTask
            {
                Day         = day,
                Time        = dt.TimeOfDay,
                Title       = title,
                Category    = category,
                IsImportant = false,
                IsUrgent    = false
            };

            if (string.IsNullOrWhiteSpace(title))
                TaskRepository.Remove(task);
            else
                TaskRepository.Upsert(task);
        }
    }
}
