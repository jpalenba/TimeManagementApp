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
    // Represents one task created in the calendar

    public partial class CalendarForm : Form
    {
        private static readonly string[] Days =
            { "Time", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        private static readonly Dictionary<string, Color> CategoryColours = new()
        {
            ["Work"]     = Color.FromArgb(0xFF, 0xC3, 0x8F),
            ["Study"]    = Color.FromArgb(0xB3, 0xD7, 0xFF),
            ["Health"]   = Color.FromArgb(0xC5, 0xE8, 0xB4),
            ["Personal"] = Color.FromArgb(0xFF, 0xE0, 0xB2)
        };

        private readonly DataGridView scheduleGrid = new();
        private readonly Panel buttonPanel      = new();
        private readonly Button btnSave         = new();
        private readonly Button btnExport       = new();
        private readonly ContextMenuStrip cellMenu = new();

        // **NEW**: in‐memory list of tasks
        private readonly List<CalendarTask> tasks = new();

        public CalendarForm(bool loadExisting = false)
        {
            KeyPreview = true;
            InitializeComponent();

            if (loadExisting && File.Exists("schedule.json"))
                LoadSchedule();
            else
                InitializeScheduleGrid();
        }

        private void InitializeComponent()
        {
            Text        = "Weekly Schedule";
            ClientSize  = new Size(880, 720);
            MinimumSize = new Size(640, 480);

            FormClosing += CalendarForm_FormClosing;
            KeyDown     += CalendarForm_KeyDown;
            Resize      += CalendarForm_Resize;

            // ─── Button panel docked BOTTOM ────────────────────────────────
            buttonPanel.Dock   = DockStyle.Bottom;
            buttonPanel.Height = 45;
            buttonPanel.Padding = new Padding(8);
            Controls.Add(buttonPanel);

            btnSave.Text  = "💾 Save (Ctrl+S)";
            btnSave.Dock  = DockStyle.Right;
            btnSave.Width = 140;
            btnSave.Click += (_,_) => { SaveSchedule(); SaveTasks(); };
            buttonPanel.Controls.Add(btnSave);

            btnExport.Text   = "⇩ Export CSV";
            btnExport.Dock   = DockStyle.Right;
            btnExport.Width  = 120;
            btnExport.Margin = new Padding(0,0,6,0);
            btnExport.Click += BtnExport_Click;
            buttonPanel.Controls.Add(btnExport);

            // ─── scheduleGrid fills the rest ────────────────────────────────
            scheduleGrid.Dock               = DockStyle.Fill;
            scheduleGrid.ReadOnly           = true;
            scheduleGrid.RowHeadersVisible  = false;
            scheduleGrid.AllowUserToAddRows    = false;
            scheduleGrid.AllowUserToDeleteRows = false;
            scheduleGrid.AllowUserToResizeRows = false;
            scheduleGrid.SelectionMode      = DataGridViewSelectionMode.CellSelect;
            scheduleGrid.Font               = new Font("Segoe UI", 10);
            scheduleGrid.BackgroundColor    = Color.White;
            scheduleGrid.DefaultCellStyle.BackColor = Color.White;

            scheduleGrid.CellDoubleClick       += ScheduleGrid_CellDoubleClick;
            scheduleGrid.CellToolTipTextNeeded += ScheduleGrid_CellToolTipTextNeeded;
            scheduleGrid.CellFormatting        += ScheduleGrid_CellFormatting;
            scheduleGrid.CellMouseClick        += ScheduleGrid_CellMouseClick;
            Controls.Add(scheduleGrid);

            // ─── context menu for clearing cells ───────────────────────────
            cellMenu.Items.Add("Clear", null, (_,_) =>
            {
                if (scheduleGrid.CurrentCell is { ColumnIndex: > 0 } c)
                    c.Value = string.Empty;
            });
        }

        private void InitializeScheduleGrid()
        {
            scheduleGrid.Columns.Clear();
            scheduleGrid.Rows.Clear();

            // 1) create columns
            foreach (var d in Days)
                scheduleGrid.Columns.Add(d, d);

            // 2) fix first column to 70px, rest fill
            scheduleGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            scheduleGrid.Columns[0].Width        = 70;
            for (int i = 1; i < scheduleGrid.Columns.Count; i++)
                scheduleGrid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // 3) add rows for 6 AM through midnight
            var dt = DateTime.Today.AddHours(6);
            var end = DateTime.Today.AddDays(1); // midnight
            while (dt <= end)
            {
                int idx = scheduleGrid.Rows.Add();
                scheduleGrid.Rows[idx].Cells[0].Value = dt.ToString("h:mm tt", CultureInfo.InvariantCulture);
                dt = dt.AddHours(1);
            }

            AdjustRowHeights();
        }

        private void LoadSchedule()
        {
            // Load grid data
            if (!File.Exists("schedule.json"))
            {
                InitializeScheduleGrid();
            }
            else
            {
                var data = JsonConvert
                    .DeserializeObject<List<string[]>>(File.ReadAllText("schedule.json"))
                    ?? new List<string[]>();

                InitializeScheduleGrid();

                // Only overwrite columns 1..7 so col 0 stays 6 AM–12 AM
                for (int r = 0; r < data.Count && r < scheduleGrid.Rows.Count; r++)
                for (int c = 1; c < data[r].Length && c < scheduleGrid.Columns.Count; c++)
                    scheduleGrid.Rows[r].Cells[c].Value = data[r][c];

                AdjustRowHeights();
            }

            // Load tasks list if you’d like
            if (File.Exists("tasks.json"))
            {
                tasks.Clear();

                var loaded = JsonConvert
                    .DeserializeObject<List<CalendarTask>>(File.ReadAllText("tasks.json"))
                    ?? new List<CalendarTask>();

                    tasks.AddRange(loaded);
            }
        }

        private void SaveSchedule()
        {
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
            MessageBox.Show("Schedule saved!", "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // **NEW**: persist tasks
       
        private void SaveTasks()
        {
            // Writes out tasks.json alongside your exe
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");

            File.WriteAllText(path,
                JsonConvert.SerializeObject(tasks, Formatting.Indented));
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

        private void CalendarForm_KeyDown(object? s, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                SaveSchedule();
                SaveTasks();
            }
        }

        private void ScheduleGrid_CellDoubleClick(object? s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;

            string day      = scheduleGrid.Columns[e.ColumnIndex].HeaderText;
            string timeText = scheduleGrid.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
            // prompt
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Enter event for {day} at {timeText}:",
                "Schedule Event",
                scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? ""
            );

            // update grid cell
            scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = input;

            // create/update/remove task
            UpsertTask(e.RowIndex, e.ColumnIndex, input);
        }

        private void UpsertTask(int row, int col, string title)
        {
            string day = scheduleGrid.Columns[col].HeaderText;
            string timeLabel = scheduleGrid.Rows[row].Cells[0].Value?.ToString() ?? "";
            if (!DateTime.TryParseExact(timeLabel, "h:mm tt", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return;

            var task = new CalendarTask {
                Day   = day,
                Time  = dt.TimeOfDay,
                Title = title
            };

            if (string.IsNullOrWhiteSpace(title))
                TaskRepository.Remove(task);
                else
                TaskRepository.Upsert(task);
            }

        private void ScheduleGrid_CellToolTipTextNeeded(object? s,
                                                        DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            string txt = scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";
            if (!string.IsNullOrWhiteSpace(txt))
                e.ToolTipText = txt;
        }

        private void ScheduleGrid_CellFormatting(object? s, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            if (e.Value is not string t || string.IsNullOrWhiteSpace(t)) return;
            int idx = t.IndexOf(':');
            if (idx <= 0) return;
            var cat = t[..idx].Trim();
            if (CategoryColours.TryGetValue(cat, out var col))
                e.CellStyle.BackColor = col;
        }

        private void ScheduleGrid_CellMouseClick(object? s, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex <= 0) return;
            var cell = scheduleGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell is not null)
            {
                scheduleGrid.CurrentCell = cell;
                cellMenu.Show(Cursor.Position);
            }
        }

        private void CalendarForm_FormClosing(object? s, FormClosingEventArgs e)
        {
            var dr = MessageBox.Show("Save changes before closing?", Text,
                                     MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.Cancel) { e.Cancel = true; return; }
            if (dr == DialogResult.Yes)
            {
                SaveSchedule();
                SaveTasks();
            }
        }

        private void CalendarForm_Resize(object? s, EventArgs e)
        {
            AdjustRowHeights();
        }

        private void AdjustRowHeights()
        {
            int rowCount = scheduleGrid.Rows.Count;
            if (rowCount == 0) return;

            int avail = ClientSize.Height
                      - buttonPanel.Height
                      - scheduleGrid.ColumnHeadersHeight;
            if (avail <= 0) return;

            int baseH = avail / rowCount;
            int extra = avail - baseH * rowCount;
            for (int i = 0; i < rowCount; i++)
                scheduleGrid.Rows[i].Height = baseH + (i < extra ? 1 : 0);
        }
    }

    internal sealed class EventDialog : Form
    {
        private readonly TextBox txt = new()
        {
            Dock      = DockStyle.Fill,
            Multiline = true,
            Font      = new Font("Segoe UI", 10)
        };
        public string EventText => txt.Text.Trim();

        public EventDialog(string current)
        {
            Text       = "Edit Event";
            ClientSize = new Size(400, 180);
            Controls.Add(txt);
            txt.Text = current;

            var panel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height        = 45,
                Padding       = new Padding(4)
            };
            Controls.Add(panel);

            var ok     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Width = 80 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };
            panel.Controls.Add(ok);
            panel.Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}
