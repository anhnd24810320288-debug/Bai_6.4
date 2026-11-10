using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GarageManager.BUS;
using GarageManager.DTO;

namespace GarageManager.GUI
{
    /// <summary>Danh sách phiếu, xem chi tiết, tính tổng chi phí, thanh toán & xuất hóa đơn.</summary>
    public class UcThanhToan : UserControl, IRefreshable
    {
        private readonly PhieuSuaChuaBUS bus = new PhieuSuaChuaBUS();
        private readonly ComboBox cboLoc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DataGridView dgvPhieu = UI.Grid();
        private readonly DataGridView dgvCT = UI.Grid();
        private readonly Label lblTong = new Label { AutoSize = false, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Firebrick };
        private bool loading;

        public UcThanhToan()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 46 };
            cboLoc.SetBounds(90, 10, 160, 24);
            cboLoc.Items.Add("(Tất cả)");
            cboLoc.Items.AddRange(TrangThaiPhieu.All);
            cboLoc.SelectedIndex = 0;
            top.Controls.AddRange(new Control[]
            {
                UI.Lbl("Trạng thái", 10, 10, 75), cboLoc,
                UI.Btn("Tải lại", 270, 7, 80, (s, e) => LoadPhieu()),
                UI.Btn("Thanh toán", 650, 7, 120, (s, e) => Run(ThanhToan)),
                UI.Btn("Xuất hóa đơn", 780, 7, 130, (s, e) => Run(XuatHoaDon))
            });

            var foot = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            lblTong.SetBounds(10, 8, 900, 28);
            foot.Controls.Add(lblTong);

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlp.Controls.Add(dgvPhieu, 0, 0);
            tlp.Controls.Add(dgvCT, 0, 1);

            Controls.Add(tlp);
            Controls.Add(foot);
            Controls.Add(top);

            cboLoc.SelectedIndexChanged += (s, e) => { if (!loading) LoadPhieu(); };
            dgvPhieu.SelectionChanged += (s, e) => { if (!loading) LoadChiTiet(); };
            RefreshData();
        }

        private PhieuSuaChuaDTO Selected() => dgvPhieu.CurrentRow?.DataBoundItem as PhieuSuaChuaDTO;

        public void RefreshData() => LoadPhieu();

        private void LoadPhieu()
        {
            try
            {
                loading = true;
                string tt = cboLoc.SelectedIndex <= 0 ? null : cboLoc.SelectedItem.ToString();
                dgvPhieu.DataSource = bus.GetAll(tt);
                UI.Headers(dgvPhieu, "MaPhieu", "Mã phiếu", "BienSo", "Biển số", "TenKH", "Chủ xe", "NgayNhan", "Ngày nhận",
                                     "NoiDungSua", "Nội dung sửa", "TongTien", "Tổng tiền (đ)", "TrangThai", "Trạng thái");
                dgvPhieu.Columns["NgayNhan"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                UI.Money(dgvPhieu, "TongTien");
            }
            catch (Exception ex) { UI.Error(ex); }
            finally { loading = false; }
            LoadChiTiet();
        }

        private void LoadChiTiet()
        {
            var p = Selected();
            if (p == null) { dgvCT.DataSource = null; lblTong.Text = ""; return; }
            try
            {
                var ct = bus.GetChiTiet(p.MaPhieu);
                dgvCT.DataSource = ct;
                UI.Headers(dgvCT, "TenPhuTung", "Phụ tùng", "SoLuong", "Số lượng", "DonGia", "Đơn giá",
                                  "TienPhuTung", "Tiền phụ tùng", "TienCong", "Tiền công", "ThanhTien", "Thành tiền");
                UI.Money(dgvCT, "DonGia", "TienPhuTung", "TienCong", "ThanhTien");
                decimal pt = ct.Sum(x => x.TienPhuTung), cong = ct.Sum(x => x.TienCong);
                lblTong.Text = $"Phiếu #{p.MaPhieu}: Phụ tùng {pt:N0}đ + Tiền công {cong:N0}đ = TỔNG {pt + cong:N0}đ   [{p.TrangThai}]";
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void ThanhToan()
        {
            var p = Selected() ?? throw new ArgumentException("Hãy chọn một phiếu.");
            if (p.TrangThai != TrangThaiPhieu.DaHoanThanh)
                throw new InvalidOperationException("Chỉ thanh toán được phiếu ở trạng thái 'Đã hoàn thành'.");
            if (!UI.Confirm($"Thanh toán phiếu #{p.MaPhieu} của xe {p.BienSo}?")) return;
            bus.ThanhToan(p.MaPhieu);
            UI.Info("Đã thanh toán. Phiếu chuyển sang 'Đã thanh toán'.");
            LoadPhieu();
        }

        private void XuatHoaDon()
        {
            var p = Selected() ?? throw new ArgumentException("Hãy chọn một phiếu.");
            string text = PhieuSuaChuaBUS.TaoHoaDon(p, bus.GetChiTiet(p.MaPhieu));
            using (var dlg = new SaveFileDialog
            {
                Filter = "Văn bản (*.txt)|*.txt",
                FileName = $"HoaDon_Phieu{p.MaPhieu}_{p.BienSo}.txt"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                File.WriteAllText(dlg.FileName, text, new UTF8Encoding(true));
                if (UI.Confirm("Đã lưu hóa đơn. Mở file ngay?"))
                    Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
        }

        private void Run(Action a)
        {
            try { a(); } catch (Exception ex) { UI.Error(ex); }
        }
    }
}
