using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TimeManagementApp.Forms;

namespace TimeManagementApp
{
    public partial class MainMenuForm : Form
    {
        // ── constants ───────────────────────────────────────────────
        private const int SidebarWidth  = 360;
        private const int TopBarHeight  = 40;
        private const int ButtonWidth   = 320;
        private const int ButtonSpacing = 6;
        private const int TopMargin     = 20;

        // ── fields ──────────────────────────────────────────────────
        private TableLayoutPanel root;
        private Panel             buttonPanel, calendarPanel;
        private TableLayoutPanel  calendarTable;
        private Timer             calendarTimer;
        private Button            btnCalendar, btnPriorityManagement, btnAnalytics, btnSettings;
        private Button            btnMinimize, btnMaxRestore, btnClose;

        public MainMenuForm()
        {
            InitializeComponent();
            Resize += (s, e) => LayoutButtons();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // ── ROOT LAYOUT ────────────────────────────────────────────
            root = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SidebarWidth));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent , 100F));
            root.RowStyles   .Add(new RowStyle   (SizeType.Absolute, TopBarHeight));
            root.RowStyles   .Add(new RowStyle   (SizeType.Percent , 100F));
            Controls.Add(root);

            // ── TITLE BAR ─────────────────────────────────────────────
            var topBar = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 47)
            };
            topBar.MouseDown += TopBar_MouseDown;
            root.Controls.Add(topBar, 0, 0);
            root.SetColumnSpan(topBar, 2);

            var lblTitle = new Label
            {
                Text      = "Time Management Hub",
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(15, 10)
            };
            topBar.Controls.Add(lblTitle);

            btnMinimize = CreateCaptionButton("_", (s, e) => WindowState = FormWindowState.Minimized);
            topBar.Controls.Add(btnMinimize);

            btnMaxRestore = CreateCaptionButton("□", (s, e) => ToggleMaxRestore());
            topBar.Controls.Add(btnMaxRestore);

            btnClose = CreateCaptionButton("X", (s, e) => Close());
            topBar.Controls.Add(btnClose);

            // ── SIDEBAR ──────────────────────────────────────────────
            var sidebar = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 63)
            };
            root.Controls.Add(sidebar, 0, 1);

            buttonPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = sidebar.BackColor
            };
            sidebar.Controls.Add(buttonPanel);

            btnCalendar           = CreateNavButton("📅 Calendar",   (s, e) => new CalendarForm(true).ShowDialog());
            btnPriorityManagement = CreateNavButton("🔥 Priorities", (s, e) => new PriorityManagementForm().ShowDialog());
            btnAnalytics          = CreateNavButton("📈 Analytics",  (s, e) => new AnalyticsForm().ShowDialog());
            btnSettings           = CreateNavButton("⚙️ Settings",   (s, e) => MessageBox.Show("Settings/Help clicked!"));

            buttonPanel.Controls.AddRange(new Control[]
            {
                btnSettings, btnAnalytics, btnPriorityManagement, btnCalendar
            });

            // ── CALENDAR PANEL ──────────────────────────────────────
            calendarPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                Padding   = new Padding(50),
                BackColor = Color.FromArgb(40, 40, 55)
            };
            root.Controls.Add(calendarPanel, 1, 1);

            var header = new Label
            {
                Text      = DateTime.Now.ToString("MMMM yyyy"),
                Dock      = DockStyle.None,
                Height    = 38,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(50, 50, 70)
            };
            calendarPanel.Controls.Add(header);

            calendarTable = new TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor       = calendarPanel.BackColor,
                ForeColor       = Color.White,
                Margin          = new Padding(10, header.Height + 5, 5, 5) 

            };
            calendarPanel.Controls.Add(calendarTable);

            calendarTimer = new Timer { Interval = 60000 };
            calendarTimer.Tick += (s, e) => DrawCalendar();
            calendarTimer.Start();
            DrawCalendar();

            // ── FORM PROPS ──────────────────────────────────────────
            ClientSize      = new Size(1080, 600);
            BackColor       = Color.FromArgb(30, 30, 47);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;

            ResumeLayout(false);
            LayoutButtons();
        }

        private Button CreateCaptionButton(string text, EventHandler onClick)
        {
            var b = new Button
            {
                Text       = text,
                Dock       = DockStyle.Right,
                Width      = 40,
                FlatStyle  = FlatStyle.Flat,
                ForeColor  = Color.FromArgb(165, 133, 255),
                BackColor  = Color.FromArgb(30, 30, 47),
                Font       = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor     = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private Button CreateNavButton(string text, EventHandler onClick)
        {
            var b = new Button
            {
                Text            = text,
                Dock            = DockStyle.Top,
                Font            = new Font("Segoe UI", 11),
                ForeColor       = Color.White,
                BackColor       = Color.FromArgb(31, 142, 241),
                FlatStyle       = FlatStyle.Flat,
                Cursor          = Cursors.Hand,
                TextAlign       = ContentAlignment.MiddleCenter
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void ToggleMaxRestore()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState        = FormWindowState.Normal;
                btnMaxRestore.Text = "□";
            }
            else
            {
                WindowState        = FormWindowState.Maximized;
                btnMaxRestore.Text = "❐";
            }
        }

        private void DrawCalendar()
        {
            calendarTable.SuspendLayout();
            calendarTable.Controls.Clear();

            DateTime today       = DateTime.Today;
            DateTime first       = new DateTime(today.Year, today.Month, 1);
            int      daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int      offset      = (int)first.DayOfWeek;
            int      totalCells  = offset + daysInMonth;
            int      dataRows    = (int)Math.Ceiling(totalCells / 7.0);

            // rebuild columns & rows
            calendarTable.ColumnStyles.Clear();
            calendarTable.RowStyles.Clear();
            calendarTable.ColumnCount = 7;
            calendarTable.RowCount    = dataRows + 1;

            float pct = 100f / 7f;
            for (int c = 0; c < 7; c++)
                calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, pct));

            float rowPct = 100f / (dataRows + 1);
            for (int r = 0; r < dataRows + 1; r++)
                calendarTable.RowStyles.Add(new RowStyle(SizeType.Percent, rowPct));

            // weekday headers
            string[] days = { "Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday" };
            for (int c = 0; c < 7; c++)
            {
                var lbl = new Label
                {
                    Text      = days[c],
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                calendarTable.Controls.Add(lbl, c, 0);
            }

            // date cells
            int col = offset, row = 1;
            for (int d = 1; d <= daysInMonth; d++)
            {
                var lbl = new Label
                {
                    Text      = d.ToString(),
                    Dock      = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font      = new Font("Segoe UI", 8),
                    ForeColor = Color.White,
                    BackColor = (d == today.Day)
                                ? Color.FromArgb(31, 142, 241)
                                : Color.Transparent
                };
                calendarTable.Controls.Add(lbl, col, row);
                col++;
                if (col == 7) { col = 0; row++; }
            }

            calendarTable.ResumeLayout();
        }

        private void LayoutButtons()
        {
            int navHeight = (WindowState == FormWindowState.Maximized) ? 260 : 140;

    int y = TopMargin;
    foreach (Control c in buttonPanel.Controls)
    {
        c.Left   = (SidebarWidth - ButtonWidth) / 2;
        c.Width  = ButtonWidth;
        c.Height = navHeight;       // use computed height
        c.Top    = y;
        y       += c.Height + ButtonSpacing;
    }
        }

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int msg, int wp, int lp);
        private void TopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
    }
}
