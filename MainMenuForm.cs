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
        private const int BottomMargin  = 20;

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

            // ROOT: 2×2 grid
            root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 2;
            root.RowCount = 2;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, SidebarWidth));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, TopBarHeight));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            // TITLE BAR
            Panel topBar = new Panel();
            topBar.Dock = DockStyle.Fill;
            topBar.BackColor = Color.FromArgb(30, 30, 47);
            topBar.MouseDown += TopBar_MouseDown;
            root.Controls.Add(topBar, 0, 0);
            root.SetColumnSpan(topBar, 2);

            Label lblTitle = new Label();
            lblTitle.Text = "Time Management Hub";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(15, 10);
            topBar.Controls.Add(lblTitle);

            btnClose = CreateCaptionButton("X");
            btnClose.Click += (s, e) => Close();
            topBar.Controls.Add(btnClose);

            btnMaxRestore = CreateCaptionButton("□");
            btnMaxRestore.Click += (s, e) => ToggleMaxRestore();
            topBar.Controls.Add(btnMaxRestore);

            btnMinimize = CreateCaptionButton("_");
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            topBar.Controls.Add(btnMinimize);

            // SIDEBAR
            Panel sidebar = new Panel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = Color.FromArgb(45, 45, 63);
            root.Controls.Add(sidebar, 0, 1);

            buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.BackColor = sidebar.BackColor;
            sidebar.Controls.Add(buttonPanel);

            btnCalendar = CreateNavButton("📅 Calendar", (s, e) => new CalendarForm(true).ShowDialog());
            btnPriorityManagement = CreateNavButton("🔥 Priorities", (s, e) => new PriorityManagementForm().ShowDialog());
            btnAnalytics = CreateNavButton("📈 Analytics", (s, e) => new AnalyticsForm().ShowDialog());
            btnSettings = CreateNavButton("⚙️ Settings", (s, e) => MessageBox.Show("Settings/Help clicked!"));

            buttonPanel.Controls.AddRange(new Control[] {
                btnCalendar, btnPriorityManagement, btnAnalytics, btnSettings
            });

            // CALENDAR PANEL
            calendarPanel = new Panel();
            calendarPanel.Dock = DockStyle.Fill;
            calendarPanel.Padding = new Padding(20);
            calendarPanel.BackColor = Color.FromArgb(40, 40, 55);
            root.Controls.Add(calendarPanel, 1, 1);

            Label header = new Label();
            header.Text = DateTime.Now.ToString("MMMM yyyy");
            header.Dock = DockStyle.Top;
            header.Height = 32;
            header.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            header.TextAlign = ContentAlignment.MiddleCenter;
            header.ForeColor = Color.White;
            header.BackColor = Color.FromArgb(50, 50, 70);
            calendarPanel.Controls.Add(header);

            calendarTable = new TableLayoutPanel();
            calendarTable.Dock = DockStyle.Fill;
            calendarTable.BackColor = calendarPanel.BackColor;
            calendarTable.ForeColor = Color.White;
            calendarTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            calendarPanel.Controls.Add(calendarTable);

            calendarTimer = new Timer();
            calendarTimer.Interval = 60000;
            calendarTimer.Tick += (s, e) => DrawCalendar();
            calendarTimer.Start();
            DrawCalendar();

            // FORM PROPS
            ClientSize = new Size(1080, 600);
            BackColor = Color.FromArgb(30, 30, 47);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;

            ResumeLayout(false);
            LayoutButtons();
        }

        private Button CreateCaptionButton(string text)
        {
            Button b = new Button();
            b.Text = text;
            b.Dock = DockStyle.Right;
            b.Width = 40;
            b.FlatStyle = FlatStyle.Flat;
            b.ForeColor = Color.FromArgb(165, 133, 255);
            b.BackColor = Color.FromArgb(30, 30, 47);
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            b.FlatAppearance.BorderSize = 0;
            b.Cursor = Cursors.Hand;
            return b;
        }

        private Button CreateNavButton(string text, EventHandler onClick)
        {
            Button b = new Button();
            b.Text = text;
            b.Dock = DockStyle.Top;
            b.Height = 100;
            b.Font = new Font("Segoe UI", 11);
            b.ForeColor = Color.White;
            b.BackColor = Color.FromArgb(31, 142, 241);
            b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            b.TextAlign = ContentAlignment.MiddleCenter;
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void ToggleMaxRestore()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                btnMaxRestore.Text = "□";
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                btnMaxRestore.Text = "❐";
            }
        }

        private void DrawCalendar()
        {
            calendarTable.SuspendLayout();
            calendarTable.Controls.Clear();

            // Weekday headers
            string[] days = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            calendarTable.ColumnCount = 7;
            calendarTable.RowCount = 0;
            calendarTable.ColumnStyles.Clear();
            foreach (string _ in days)
                calendarTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 7F));

            for (int i = 0; i < days.Length; i++)
            {
                Label lbl = new Label();
                lbl.Text = days[i];
                lbl.Dock = DockStyle.Fill;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                calendarTable.Controls.Add(lbl, i, 0);
            }

            DateTime today = DateTime.Today;
            DateTime firstOfMonth = new DateTime(today.Year, today.Month, 1);
            int offset = (int)firstOfMonth.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            int totalCells = offset + daysInMonth;
            int dateRows = (int)Math.Ceiling(totalCells / 7.0);

            calendarTable.RowCount = 1 + dateRows;
            calendarTable.RowStyles.Clear();
            calendarTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            for (int r = 0; r < dateRows; r++)
                calendarTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            int col = offset, row = 1;
            for (int d = 1; d <= daysInMonth; d++)
            {
                Label lbl = new Label();
                lbl.Text = d.ToString();
                lbl.Dock = DockStyle.Fill;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Segoe UI", 8);
                lbl.ForeColor = Color.White;
                lbl.BackColor = (d == today.Day) ? Color.FromArgb(31, 142, 241) : Color.Transparent;
                calendarTable.Controls.Add(lbl, col, row);

                col++;
                if (col > 6)
                {
                    col = 0;
                    row++;
                }
            }

            calendarTable.ResumeLayout();
        }

        private void LayoutButtons()
        {
            int y = TopMargin;
            foreach (Control c in buttonPanel.Controls)
            {
                c.Left = (SidebarWidth - ButtonWidth) / 2;
                c.Width = ButtonWidth;
                c.Top = y;
                y += c.Height + ButtonSpacing;
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
