using System;
using System.Drawing;
using System.Windows.Forms;
using GarageManager.BUS;
using GarageManager.DTO;

namespace GarageManager.GUI
{
    /// <summary>Tiếp nhận xe & quản lý chủ xe (CRUD).</summary>
    public class UcXe : UserControl, IRefreshable
    {
        private readonly XeBUS bus = new XeBUS();
        private readonly KhachHangBUS khBus = new KhachHangBUS();
        private readonly DataGridView dgv = UI.Grid();
        private readonly TextBox txtBS = new TextBox(), txtTen = new TextBox(), txtSDT = new TextBox(), txtDC = new TextBox();
        private readonly ComboBox cboHieu = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        private readonly NumericUpDown nudNam;
        private readonly Label lblTT = new Label { ForeColor = Color.DarkBlue, AutoSize = false };
        private XeDTO current;
        private bool loading;

        public UcXe()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 165 };
            txtBS.SetBounds(100, 12, 150, 24);
            cboHieu.SetBounds(100, 48, 150, 24);
            cboHieu.Items.AddRange(new object[] { "Toyota", "Honda", "VinFast", "Hyundai", "Kia", "Mazda", "Ford", "Mercedes", "BMW" });
            nudNam = UI.Num(350, 48, 100, DateTime.Now.Year + 1);
            nudNam.Minimum = 1950; nudNam.Value = DateTime.Now.Year;
            txtTen.SetBounds(100, 84, 250, 24);
            txtSDT.SetBounds(450, 84, 150, 24);
            txtDC.SetBounds(100, 120, 500, 24);
            lblTT.SetBounds(360, 12, 420, 26);

            pnl.Controls.AddRange(new Control[]
            {
                UI.Lbl("Biển số", 10, 12), txtBS,
                UI.Btn("Tra cứu", 260, 9, 85, (s, e) => Run(TraCuu, false)),
                lblTT,
                UI.Lbl("Hiệu xe", 10, 48), cboHieu,
                UI.Lbl("Năm SX", 270, 48, 70), nudNam,
                UI.Lbl("Tên chủ xe", 10, 84), txtTen,
                UI.Lbl("Điện thoại", 365, 84, 80), txtSDT,
                UI.Lbl("Địa chỉ", 10, 120), txtDC,
                UI.Btn("Tiếp nhận xe mới", 800, 8, 150, (s, e) => Run(TiepNhan, true)),
                UI.Btn("Cập nhật", 800, 44, 150, (s, e) => Run(CapNhat, true)),
                UI.Btn("Xóa xe", 800, 80, 150, (s, e) => Run(XoaXe, true)),
                UI.Btn("Xóa chủ xe", 800, 116, 150, (s, e) => Run(XoaChuXe, true)),
                UI.Btn("Làm mới", 960, 8, 90, (s, e) => { Clear(); RefreshData(); })
            });

            Controls.Add(dgv);
            Controls.Add(pnl);
            dgv.SelectionChanged += (s, e) =>
            {
                if (!loading && dgv.CurrentRow?.DataBoundItem is XeDTO x) { Fill(x); lblTT.Text = "Đã chọn xe " + x.BienSo; }
            };
            RefreshData();
        }

        private void Fill(XeDTO x)
        {
            current = x;
            txtBS.Text = x.BienSo; cboHieu.Text = x.HieuXe;
            if (x.NamSanXuat.HasValue && x.NamSanXuat >= nudNam.Minimum && x.NamSanXuat <= nudNam.Maximum)
                nudNam.Value = x.NamSanXuat.Value;
            txtTen.Text = x.TenKH; txtSDT.Text = x.DienThoai; txtDC.Text = x.DiaChi;
        }

        private void Clear()
        {
            current = null;
            txtBS.Clear(); cboHieu.Text = ""; txtTen.Clear(); txtSDT.Clear(); txtDC.Clear();
            nudNam.Value = DateTime.Now.Year; lblTT.Text = "";
        }

        private XeDTO ReadXe() => new XeDTO
        {
            BienSo = txtBS.Text, HieuXe = cboHieu.Text.Trim(), NamSanXuat = (int)nudNam.Value
        };

        private KhachHangDTO ReadKH(int ma) => new KhachHangDTO
        {
            MaKH = ma, TenKH = txtTen.Text.Trim(), DienThoai = txtSDT.Text.Trim(), DiaChi = txtDC.Text.Trim()
        };

        // ---- nghiệp vụ ----
        private void TraCuu()
        {
            var x = bus.TraCuu(txtBS.Text);   // truy vấn dùng SqlParameter
            if (x == null)
            {
                current = null;
                lblTT.ForeColor = Color.DarkOrange;
                lblTT.Text = "Xe mới - hãy nhập thông tin chủ xe rồi bấm 'Tiếp nhận xe mới'.";
            }
            else
            {
                Fill(x);
                lblTT.ForeColor = Color.SeaGreen;
                lblTT.Text = "Xe đã có trong hệ thống - chủ xe: " + x.TenKH;
            }
        }

        private void TiepNhan()
        {
            bus.TiepNhanXeMoi(ReadKH(0), ReadXe());
            UI.Info("Đã tiếp nhận xe " + XeBUS.ChuanHoaBienSo(txtBS.Text) + ".");
        }

        private void CapNhat()
        {
            if (current == null) throw new ArgumentException("Hãy tra cứu hoặc chọn một xe có sẵn để cập nhật.");
            var xe = ReadXe(); xe.BienSo = current.BienSo;
            bus.CapNhat(ReadKH(current.MaKH), xe);
            UI.Info("Đã cập nhật thông tin xe và chủ xe.");
        }

        private void XoaXe()
        {
            if (current == null) throw new ArgumentException("Hãy chọn xe cần xóa.");
            if (UI.Confirm("Xóa xe " + current.BienSo + "?")) { bus.Xoa(current.BienSo); Clear(); }
        }

        private void XoaChuXe()
        {
            if (current == null) throw new ArgumentException("Hãy chọn xe của chủ xe cần xóa.");
            if (UI.Confirm("Xóa chủ xe \"" + current.TenKH + "\"? (chỉ xóa được khi chủ xe không còn xe nào)"))
            { khBus.Xoa(current.MaKH); Clear(); }
        }

        private void Run(Action a, bool reload)
        {
            try { a(); if (reload) RefreshData(); } catch (Exception ex) { UI.Error(ex); }
        }

        public void RefreshData()
        {
            try
            {
                loading = true;
                dgv.DataSource = bus.GetAll();
                UI.Headers(dgv, "BienSo", "Biển số", "HieuXe", "Hiệu xe", "NamSanXuat", "Năm SX",
                                "TenKH", "Chủ xe", "DienThoai", "Điện thoại", "DiaChi", "Địa chỉ");
                dgv.ClearSelection();
            }
            catch (Exception ex) { UI.Error(ex); }
            finally { loading = false; }
        }
    }
}
