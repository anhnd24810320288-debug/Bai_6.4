using System.Windows.Forms;

namespace GarageManager.GUI
{
    public class FrmMain : Form
    {
        public FrmMain()
        {
            Text = "Quản lý Garage & Bảo dưỡng Xe ô tô";
            Width = 1200; Height = 740;
            StartPosition = FormStartPosition.CenterScreen;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(Page("Tiếp nhận xe & Chủ xe", new UcXe()));
            tabs.TabPages.Add(Page("Phụ tùng & Kho", new UcPhuTung()));
            tabs.TabPages.Add(Page("Lập phiếu sửa chữa", new UcPhieuSua()));
            tabs.TabPages.Add(Page("Thanh toán & Hóa đơn", new UcThanhToan()));
            tabs.TabPages.Add(Page("Thống kê", new UcThongKe()));

            // Tải lại dữ liệu mỗi khi chuyển tab để thấy thay đổi từ tab khác
            tabs.SelectedIndexChanged += (s, e) =>
            {
                if (tabs.SelectedTab.Controls.Count > 0 && tabs.SelectedTab.Controls[0] is IRefreshable r)
                    r.RefreshData();
            };
            Controls.Add(tabs);
        }

        private static TabPage Page(string title, UserControl uc)
        {
            var p = new TabPage(title);
            uc.Dock = DockStyle.Fill;
            p.Controls.Add(uc);
            return p;
        }
    }
}
