using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TimeManagementApp.Models;
using TimeManagementApp.Services;

namespace TimeManagementApp.Forms
{
    public class PriorityManagementForm : BaseForm
    {
        private TableLayoutPanel matrix;
        private ListBox lbIU, lbIN, lbNU, lbNN;
        private ContextMenuStrip itemMenu;

        public PriorityManagementForm()
        {
            Text       = "Priority Management";
            ClientSize = new Size(800, 600);
            InitializeComponent();
            LoadMatrix();
        }

        private void InitializeComponent()
        {
            matrix = new TableLayoutPanel {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2,
                BackColor   = BackColor
            };
            matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            matrix.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            matrix.RowStyles   .Add(new RowStyle   (SizeType.Percent, 50F));
            matrix.RowStyles   .Add(new RowStyle   (SizeType.Percent, 50F));
            Controls.Add(matrix);

            lbIU = CreateQuadrant("Important & Urgent");
            lbIN = CreateQuadrant("Important & Not Urgent");
            lbNU = CreateQuadrant("Not Important & Urgent");
            lbNN = CreateQuadrant("Not Important & Not Urgent");

            matrix.Controls.Add(lbIU.Parent, 0, 0);
            matrix.Controls.Add(lbIN.Parent, 1, 0);
            matrix.Controls.Add(lbNU.Parent, 0, 1);
            matrix.Controls.Add(lbNN.Parent, 1, 1);

            itemMenu = new ContextMenuStrip { BackColor = BackColor, ForeColor = ForeColor };
            itemMenu.Items.Add("Toggle Important", null, ToggleImportant_Click);
            itemMenu.Items.Add("Toggle Urgent",    null, ToggleUrgent_Click);

            foreach (var lb in new[] { lbIU, lbIN, lbNU, lbNN })
            {
                lb.MouseDown   += ListBox_MouseDown;
                lb.MouseDown   += ListBox_DragStart;
                lb.DragEnter   += ListBox_DragEnter;
                lb.DragDrop    += ListBox_DragDrop;
            }
        }

        private ListBox CreateQuadrant(string title)
        {
            var gb = new GroupBox {
                Text      = title,
                Dock      = DockStyle.Fill,
                Font      = Font,
                ForeColor = ForeColor,
                BackColor = BackColor,
                Margin    = new Padding(4)
            };
            var lb = new ListBox {
                Dock      = DockStyle.Fill,
                Font      = Font,
                Margin    = new Padding(4),
                AllowDrop = true
            };
            gb.Controls.Add(lb);
            return lb;
        }

        private void LoadMatrix()
        {
            lbIU.Items.Clear();
            lbIN.Items.Clear();
            lbNU.Items.Clear();
            lbNN.Items.Clear();

            var tasks = TaskRepository.Tasks;

            void AddDistinctTitles(ListBox lb, Func<CalendarTask, bool> pred)
            {
                var titles = tasks.Where(pred)
                                  .Select(t => t.Title)
                                  .Distinct();
                foreach (var title in titles)
                    lb.Items.Add(title);
            }

            AddDistinctTitles(lbIU, t => t.IsImportant && t.IsUrgent);
            AddDistinctTitles(lbIN, t => t.IsImportant && !t.IsUrgent);
            AddDistinctTitles(lbNU, t => !t.IsImportant && t.IsUrgent);
            AddDistinctTitles(lbNN, t => !t.IsImportant && !t.IsUrgent);
        }

        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var lb = (ListBox)sender;
                int idx = lb.IndexFromPoint(e.Location);
                if (idx >= 0)
                {
                    lb.SelectedIndex = idx;
                    itemMenu.Show(lb, e.Location);
                }
            }
        }

        private void ListBox_DragStart(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var lb = (ListBox)sender;
            int idx = lb.IndexFromPoint(e.Location);
            if (idx < 0) return;
            var title = lb.Items[idx] as string;
            lb.DoDragDrop(title, DragDropEffects.Move);
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
                e.Effect = DragDropEffects.Move;
        }

        private void ListBox_DragDrop(object sender, DragEventArgs e)
        {
            var target = (ListBox)sender;
            var title  = e.Data.GetData(typeof(string)) as string;
            if (title == null) return;

            bool imp = target == lbIU || target == lbIN;
            bool urg = target == lbIU || target == lbNU;

            foreach (var t in TaskRepository.Tasks.Where(t => t.Title == title))
            {
                t.IsImportant = imp;
                t.IsUrgent    = urg;
                TaskRepository.Upsert(t);
            }
            LoadMatrix();
        }

        private void ToggleImportant_Click(object sender, EventArgs e)
        {
            var lb = GetCurrentListBox();
            if (lb?.SelectedItem is string title)
            {
                foreach (var t in TaskRepository.Tasks.Where(t => t.Title == title))
                    t.IsImportant = !t.IsImportant;
                TaskRepository.Save();
                LoadMatrix();
            }
        }

        private void ToggleUrgent_Click(object sender, EventArgs e)
        {
            var lb = GetCurrentListBox();
            if (lb?.SelectedItem is string title)
            {
                foreach (var t in TaskRepository.Tasks.Where(t => t.Title == title))
                    t.IsUrgent = !t.IsUrgent;
                TaskRepository.Save();
                LoadMatrix();
            }
        }

        private ListBox GetCurrentListBox()
        {
            if (lbIU.Focused) return lbIU;
            if (lbIN.Focused) return lbIN;
            if (lbNU.Focused) return lbNU;
            if (lbNN.Focused) return lbNN;
            return null;
        }
    }
}
  