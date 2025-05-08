using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TimeManagementApp.Models;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public class PriorityManagementForm : Form
    {
        private TableLayoutPanel matrix;
        private ListBox lbIU, lbIN, lbNU, lbNN;     // four quadrants
        private ContextMenuStrip itemMenu;

        public PriorityManagementForm()
        {
            InitializeComponent();
            LoadMatrix();
        }

        private void InitializeComponent()
        {
            Text       = "Priority Management";
            ClientSize = new Size(800, 600);

            // 1) Matrix layout: 2x2
            matrix = new TableLayoutPanel {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2
            };
            // equally divide
            matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            matrix.RowStyles   .Add(new RowStyle   (SizeType.Percent, 50F));
            matrix.RowStyles   .Add(new RowStyle   (SizeType.Percent, 50F));
            Controls.Add(matrix);

            // 2) Build each quadrant as a GroupBox + ListBox
            lbIU = CreateQuadrant("Important & Urgent");
            lbIN = CreateQuadrant("Important & Not Urgent");
            lbNU = CreateQuadrant("Not Important & Urgent");
            lbNN = CreateQuadrant("Not Important & Not Urgent");

            matrix.Controls.Add(lbIU.Parent, 0, 0);
            matrix.Controls.Add(lbIN.Parent, 1, 0);
            matrix.Controls.Add(lbNU.Parent, 0, 1);
            matrix.Controls.Add(lbNN.Parent, 1, 1);

            // 3) Shared context‐menu to toggle flags
            itemMenu = new ContextMenuStrip();
            itemMenu.Items.Add("Toggle Important", null, ToggleImportant_Click);
            itemMenu.Items.Add("Toggle Urgent",    null, ToggleUrgent_Click);

            // Hook right‐click to show the menu on any ListBox
            foreach (var lb in new[] { lbIU, lbIN, lbNU, lbNN })
                lb.MouseDown += ListBox_MouseDown;
        }

        // Factory for quadrant + label
        private ListBox CreateQuadrant(string title)
        {
            var gb = new GroupBox {
                Text     = title,
                Dock     = DockStyle.Fill,
                Margin   = new Padding(4)
            };
            var lb = new ListBox {
                Dock   = DockStyle.Fill,
                Font   = new Font("Segoe UI", 10),
                Margin = new Padding(4)
            };
            gb.Controls.Add(lb);
            return lb;
        }

        // Populate each ListBox from the shared TaskRepository.Tasks
        private void LoadMatrix()
        {
            // clear
            lbIU.Items.Clear();
            lbIN.Items.Clear();
            lbNU.Items.Clear();
            lbNN.Items.Clear();

            foreach (var t in TaskRepository.Tasks)
            {
                string label = $"{t.Title} ({t.Day} {DateTime.Today.Add(t.Time):h:mm tt})";
                if (t.IsImportant && t.IsUrgent)          lbIU.Items.Add(t);
                else if (t.IsImportant && !t.IsUrgent)    lbIN.Items.Add(t);
                else if (!t.IsImportant && t.IsUrgent)    lbNU.Items.Add(t);
                else                                       lbNN.Items.Add(t);
            }

            // show text properly
            foreach (var lb in new[] { lbIU, lbIN, lbNU, lbNN })
                lb.DisplayMember = "Title";
        }

        // On right-click, select item under cursor and show menu
        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var lb = (ListBox)sender;
            int idx = lb.IndexFromPoint(e.Location);
            if (idx < 0) return;
            lb.SelectedIndex = idx;
            itemMenu.Show(lb, e.Location);
        }

        // Menu handlers toggle the corresponding flag and refresh
        private void ToggleImportant_Click(object sender, EventArgs e)
        {
            if (!(GetCurrentListBox()?.SelectedItem is CalendarTask t)) return;
            t.IsImportant = !t.IsImportant;
            TaskRepository.Upsert(t);
            LoadMatrix();
        }

        private void ToggleUrgent_Click(object sender, EventArgs e)
        {
            if (!(GetCurrentListBox()?.SelectedItem is CalendarTask t)) return;
            t.IsUrgent = !t.IsUrgent;
            TaskRepository.Upsert(t);
            LoadMatrix();
        }

        // Helper to find which ListBox currently has focus
        private ListBox GetCurrentListBox()
        {
            return lbIU.Focused  ? lbIU
                 : lbIN.Focused  ? lbIN
                 : lbNU.Focused  ? lbNU
                 : lbNN.Focused  ? lbNN
                                 : null;
        }
    }
}
