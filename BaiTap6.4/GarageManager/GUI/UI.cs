using System;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using GarageManager.DTO;

namespace GarageManager.GUI
{
    public interface IRefreshable { void RefreshData(); }

    /// <summary>Hàm tiện ích dựng giao diện bằng code (không cần Designer).</summary>
    internal static class UI
    {
        public static Label Lbl(string text, int x, int y, int w = 90)
            => new Label { Text = text, Left = x, Top = y + 4, Width = w };

        public static Button Btn(string text, int x, int y, int w, EventHandler click)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 30 };
            b.Click += click;
            return b;
        }

        public static NumericUpDown Num(int x, int y, int w, decimal max, decimal inc = 1, bool thousands = false)
            => new NumericUpDown
            {
                Left = x, Top = y, Width = w, Minimum = 0, Maximum = max, Increment = inc,
                ThousandsSeparator = thousands, DecimalPlaces = 0
            };

        public static DataGridView Grid() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            BackgroundColor = Color.White
        };

        /// <summary>Đặt tiêu đề cột theo cặp (tênCột, tiêu đề); cột không liệt kê sẽ bị ẩn.</summary>
        public static void Headers(DataGridView g, params string[] pairs)
        {
            foreach (DataGridViewColumn c in g.Columns) c.Visible = false;
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                if (g.Columns.Contains(pairs[i]))
                {
                    var col = g.Columns[pairs[i]];
                    col.HeaderText = pairs[i + 1];
                    col.Visible = true;
                    col.DisplayIndex = i / 2;
                }
            }
        }

        public static void Money(DataGridView g, params string[] cols)
        {
            foreach (var c in cols)
                if (g.Columns.Contains(c))
                {
                    g.Columns[c].DefaultCellStyle.Format = "N0";
                    g.Columns[c].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
        }

        public static void Error(Exception ex)
        {
            if (ex is KhoKhongDuException kk)
            {
                MessageBox.Show(kk.Message +
                    "\n\nToàn bộ giao dịch đã được hoàn tác (ROLLBACK): phiếu sửa chữa KHÔNG được tạo và kho không bị trừ.",
                    "Không đủ phụ tùng trong kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string msg = ex.Message;
            if (ex is SqlException se)
            {
                if (se.Number == 2627 || se.Number == 2601) msg = "Dữ liệu bị trùng (ví dụ: biển số hoặc số điện thoại đã tồn tại).";
                else if (se.Number == 547) msg = "Không thực hiện được do ràng buộc dữ liệu (đang được sử dụng ở bảng khác, hoặc giá trị không hợp lệ).";
                else msg = "Lỗi cơ sở dữ liệu: " + se.Message;
            }
            MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Info(string msg)
            => MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static bool Confirm(string msg)
            => MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
