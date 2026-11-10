using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GarageManager.BUS;
using GarageManager.DTO;

namespace GarageManager.GUI
{
    /// <summary>Lập phiếu sửa chữa, thêm phụ tùng/tiền công, xác nhận hoàn tất (SqlTransaction ở DAL).</summary>
    public class UcPhieuSua : UserControl, IRefreshable
    {
        private readonly XeBUS xeBus = new XeBUS();
        private readonly PhuTungBUS ptBus = new PhuTungBUS();
        private readonly PhieuSuaChuaBUS pcBus = new PhieuSuaChuaBUS();

        private readonly TextBox txtBS = new TextBox();
        private readonly Label lblXe = new Label { AutoSize = false, ForeColor = Color.DarkBlue };
        private readonly TextBox txtND = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly ComboBox cboPT = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown nudSL = UI.Num(500, 12, 60, 10000);
        private readonly NumericUpDown nudCong = UI.Num(650, 12, 120, 1000000000, 10000, true);
        private readonly BindingList<ChiTietSuaChuaDTO> dong = new BindingList<ChiTietSuaChuaDTO>();
        private readonly DataGridView dgv = UI.Grid();
        private readonly Label lblTong = new Label { AutoSize = false, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.Firebrick };
        private XeDTO xe;

        public UcPhieuSua()
        {
            // --- Khu vực chọn xe + nội dung (trên cùng) ---
            var head = new Panel { Dock = DockStyle.Top, Height = 120 };
            txtBS.SetBounds(100, 12, 150, 24);
            lblXe.SetBounds(360, 12, 700, 26);
            txtND.SetBounds(100, 48, 700, 60);
            head.Controls.AddRange(new Control[]
            {
                UI.Lbl("Biển số xe", 10, 12), txtBS,
                UI.Btn("Tra cứu xe", 260, 9, 90, (s, e) => Run(TraCuuXe)),
                lblXe,
                UI.Lbl("Nội dung sửa", 10, 48), txtND
            });

            // --- Dòng nhập phụ tùng ---
            var line = new Panel { Dock = DockStyle.Top, Height = 48 };
            nudSL.Minimum = 1; nudSL.Value = 1;
            cboPT.SetBounds(100, 12, 330, 24);
            line.Controls.AddRange(new Control[]
            {
                UI.Lbl("Phụ tùng", 10, 12, 80), cboPT,
                UI.Lbl("SL", 450, 12, 40), nudSL,
                UI.Lbl("Tiền công", 580, 12, 70), nudCong,
                UI.Btn("Thêm dòng", 790, 8, 100, (s, e) => Run(ThemDong)),
                UI.Btn("Xóa dòng", 900, 8, 90, (s, e) => Run(XoaDong))
            });

            // --- Thanh dưới: tổng tiền + xác nhận ---
            var foot = new Panel { Dock = DockStyle.Bottom, Height = 56 };
            lblTong.SetBounds(10, 14, 520, 28);
            foot.Controls.AddRange(new Control[]
            {
                lblTong,
                UI.Btn("Xác nhận hoàn tất phiếu", 560, 12, 200, (s, e) => Run(XacNhan)),
                UI.Btn("Làm mới phiếu", 770, 12, 120, (s, e) => { ResetPhieu(); ReloadPhuTung(); })
            });

            dgv.DataSource = dong;
            UI.Headers(dgv, "TenPhuTung", "Phụ tùng", "SoLuong", "Số lượng", "DonGia", "Đơn giá",
                            "TienPhuTung", "Tiền phụ tùng", "TienCong", "Tiền công", "ThanhTien", "Thành tiền");
            UI.Money(dgv, "DonGia", "TienPhuTung", "TienCong", "ThanhTien");
            dong.ListChanged += (s, e) => TinhTong();

            // Thứ tự add: Fill → Bottom → Top (dưới) → Top (trên cùng thêm sau cùng)
            Controls.Add(dgv);
            Controls.Add(foot);
            Controls.Add(line);
            Controls.Add(head);

            TinhTong();
            ReloadPhuTung();
        }

        private void TraCuuXe()
        {
            xe = xeBus.TraCuu(txtBS.Text);
            if (xe == null)
            {
                lblXe.ForeColor = Color.Firebrick;
                lblXe.Text = "Xe chưa có trong hệ thống - hãy tiếp nhận xe ở tab 'Tiếp nhận xe & Chủ xe'.";
            }
            else
            {
                lblXe.ForeColor = Color.SeaGreen;
                lblXe.Text = $"{xe.HieuXe} {xe.NamSanXuat} | Chủ xe: {xe.TenKH} - {xe.DienThoai}";
            }
        }

        private void ThemDong()
        {
            var pt = cboPT.SelectedItem as PhuTungDTO;
            if (pt == null) throw new ArgumentException("Hãy chọn phụ tùng.");
            int sl = (int)nudSL.Value;
            decimal cong = nudCong.Value;

            var da = dong.FirstOrDefault(d => d.MaPhuTung == pt.MaPhuTung);
            if (da != null)
            {
                da.SoLuong += sl; da.TienCong += cong;
                dong.ResetBindings();
            }
            else
            {
                dong.Add(new ChiTietSuaChuaDTO
                {
                    MaPhuTung = pt.MaPhuTung, TenPhuTung = pt.TenPhuTung,
                    SoLuong = sl, DonGia = pt.DonGia, TienCong = cong
                });
            }
        }

        private void XoaDong()
        {
            if (dgv.CurrentRow?.DataBoundItem is ChiTietSuaChuaDTO d) dong.Remove(d);
        }

        private void XacNhan()
        {
            if (xe == null)
                throw new ArgumentException("Hãy tra cứu biển số xe hợp lệ trước (xe mới phải tiếp nhận ở tab 'Tiếp nhận xe').");
            if (!UI.Confirm($"Xác nhận hoàn tất phiếu sửa chữa cho xe {xe.BienSo}?\nKho phụ tùng sẽ được trừ tương ứng."))
                return;

            int ma = pcBus.LapPhieu(xe.BienSo, txtND.Text, dong.ToList());   // SqlTransaction trong DAL
            UI.Info($"Đã lập phiếu #{ma} (trạng thái 'Đã hoàn thành') và trừ kho phụ tùng.");
            ResetPhieu();
            ReloadPhuTung();
        }

        private void ResetPhieu()
        {
            xe = null; txtBS.Clear(); lblXe.Text = ""; txtND.Clear();
            dong.Clear(); nudSL.Value = 1; nudCong.Value = 0;
        }

        private void TinhTong()
        {
            decimal pt = dong.Sum(d => d.TienPhuTung), cong = dong.Sum(d => d.TienCong);
            lblTong.Text = $"Phụ tùng: {pt:N0}đ  +  Công: {cong:N0}đ  =  TỔNG: {pt + cong:N0}đ";
        }

        private void ReloadPhuTung()
        {
            try
            {
                int old = (cboPT.SelectedItem as PhuTungDTO)?.MaPhuTung ?? 0;
                cboPT.Items.Clear();
                foreach (var p in ptBus.GetAll()) cboPT.Items.Add(p);
                if (cboPT.Items.Count > 0) cboPT.SelectedIndex = 0;
                foreach (PhuTungDTO p in cboPT.Items) if (p.MaPhuTung == old) cboPT.SelectedItem = p;
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void Run(Action a)
        {
            try { a(); } catch (Exception ex) { UI.Error(ex); }
        }

        public void RefreshData() => ReloadPhuTung();
    }
}
