using System;
using System.Drawing;
using System.Windows.Forms;
using GarageManager.BUS;

namespace GarageManager.GUI
{
    /// <summary>Thống kê doanh thu theo tháng và phụ tùng sắp hết hàng.</summary>
    public class UcThongKe : UserControl, IRefreshable
    {
        private readonly ThongKeBUS tkBus = new ThongKeBUS();
        private readonly PhuTungBUS ptBus = new PhuTungBUS();
        private readonly DataGridView dgvDT = UI.Grid();
        private readonly DataGridView dgvTon = UI.Grid();
        private readonly NumericUpDown nudNguong = UI.Num(430, 10, 60, 1000);
        private readonly Label lblDT = new Label { Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.SeaGreen };
        private readonly Label lblTon = new Label { Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Firebrick };

        public UcThongKe()
        {
            nudNguong.Value = 5;
            var top = new Panel { Dock = DockStyle.Top, Height = 46 };
            top.Controls.AddRange(new Control[]
            {
                UI.Btn("Làm mới", 10, 7, 90, (s, e) => RefreshData()),
                UI.Lbl("Cảnh báo phụ tùng có tồn kho ≤", 120, 10, 300), nudNguong
            });

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            var p1 = new Panel { Dock = DockStyle.Fill };
            var p2 = new Panel { Dock = DockStyle.Fill };
            p1.Controls.Add(dgvDT);  p1.Controls.Add(lblDT);
            p2.Controls.Add(dgvTon); p2.Controls.Add(lblTon);
            tlp.Controls.Add(p1, 0, 0);
            tlp.Controls.Add(p2, 0, 1);

            Controls.Add(tlp);
            Controls.Add(top);
            nudNguong.ValueChanged += (s, e) => LoadTon();
            RefreshData();
        }

        public void RefreshData()
        {
            try
            {
                dgvDT.DataSource = tkBus.DoanhThuTheoThang();
                UI.Headers(dgvDT, "Thang", "Tháng", "SoPhieu", "Số phiếu đã thanh toán", "DoanhThu", "Doanh thu (đ)");
                UI.Money(dgvDT, "DoanhThu");
                lblDT.Text = $"Doanh thu theo tháng - Tổng cộng: {tkBus.TongDoanhThu():N0} đ";
            }
            catch (Exception ex) { UI.Error(ex); }
            LoadTon();
        }

        private void LoadTon()
        {
            try
            {
                var ds = ptBus.GetTonThap((int)nudNguong.Value);
                dgvTon.DataSource = ds;
                UI.Headers(dgvTon, "MaPhuTung", "Mã", "TenPhuTung", "Tên phụ tùng", "DonGia", "Đơn giá (đ)", "SoLuongTon", "Tồn kho");
                UI.Money(dgvTon, "DonGia");
                lblTon.Text = $"Phụ tùng sắp hết hàng (tồn ≤ {nudNguong.Value}): {ds.Count} mặt hàng";
            }
            catch (Exception ex) { UI.Error(ex); }
        }
    }
}
