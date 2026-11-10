using System;
using System.Windows.Forms;
using GarageManager.BUS;
using GarageManager.DTO;

namespace GarageManager.GUI
{
    /// <summary>Quản lý danh mục phụ tùng, cập nhật đơn giá và tồn kho.</summary>
    public class UcPhuTung : UserControl, IRefreshable
    {
        private readonly PhuTungBUS bus = new PhuTungBUS();
        private readonly DataGridView dgv = UI.Grid();
        private readonly TextBox txtTen = new TextBox();
        private readonly NumericUpDown nudGia = UI.Num(100, 48, 160, 1000000000, 1000, true);
        private readonly NumericUpDown nudTon = UI.Num(100, 84, 100, 1000000);
        private int ma = 0;
        private bool loading;

        public UcPhuTung()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            txtTen.SetBounds(100, 12, 350, 24);
            pnl.Controls.AddRange(new Control[]
            {
                UI.Lbl("Tên phụ tùng", 10, 12), txtTen,
                UI.Lbl("Đơn giá (đ)", 10, 48), nudGia,
                UI.Lbl("Số lượng tồn", 10, 84), nudTon,
                UI.Btn("Thêm", 500, 10, 90, (s, e) => Run(() => { bus.Them(Read()); UI.Info("Đã thêm phụ tùng."); })),
                UI.Btn("Sửa / Cập nhật", 500, 48, 140, (s, e) => Run(() => { bus.Sua(Read()); UI.Info("Đã cập nhật giá và tồn kho."); })),
                UI.Btn("Xóa", 500, 86, 90, (s, e) => Run(() =>
                {
                    if (ma > 0 && UI.Confirm("Xóa phụ tùng này?")) { bus.Xoa(ma); Clear(); }
                })),
                UI.Btn("Làm mới", 600, 10, 90, (s, e) => { Clear(); RefreshData(); })
            });
            Controls.Add(dgv);
            Controls.Add(pnl);
            dgv.SelectionChanged += (s, e) =>
            {
                if (loading) return;
                if (dgv.CurrentRow?.DataBoundItem is PhuTungDTO p)
                {
                    ma = p.MaPhuTung; txtTen.Text = p.TenPhuTung;
                    nudGia.Value = Math.Min(p.DonGia, nudGia.Maximum);
                    nudTon.Value = Math.Min(p.SoLuongTon, nudTon.Maximum);
                }
            };
            RefreshData();
        }

        private PhuTungDTO Read() => new PhuTungDTO
        {
            MaPhuTung = ma, TenPhuTung = txtTen.Text.Trim(), DonGia = nudGia.Value, SoLuongTon = (int)nudTon.Value
        };

        private void Clear() { ma = 0; txtTen.Clear(); nudGia.Value = 0; nudTon.Value = 0; }

        private void Run(Action a)
        {
            try { a(); RefreshData(); } catch (Exception ex) { UI.Error(ex); }
        }

        public void RefreshData()
        {
            try
            {
                loading = true;
                dgv.DataSource = bus.GetAll();
                UI.Headers(dgv, "MaPhuTung", "Mã", "TenPhuTung", "Tên phụ tùng", "DonGia", "Đơn giá (đ)", "SoLuongTon", "Tồn kho");
                UI.Money(dgv, "DonGia");
                dgv.ClearSelection();
            }
            catch (Exception ex) { UI.Error(ex); }
            finally { loading = false; }
        }
    }
}
