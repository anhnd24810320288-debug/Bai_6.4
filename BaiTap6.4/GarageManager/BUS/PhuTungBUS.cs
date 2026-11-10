using System;
using System.Collections.Generic;
using GarageManager.DAL;
using GarageManager.DTO;

namespace GarageManager.BUS
{
    public class PhuTungBUS
    {
        private readonly PhuTungDAL dal = new PhuTungDAL();

        public List<PhuTungDTO> GetAll() => dal.GetAll();
        public List<PhuTungDTO> GetTonThap(int nguong) => dal.GetTonThap(nguong);

        private static void Validate(PhuTungDTO p)
        {
            if (string.IsNullOrWhiteSpace(p.TenPhuTung)) throw new ArgumentException("Tên phụ tùng không được để trống.");
            if (p.TenPhuTung.Length > 150) throw new ArgumentException("Tên phụ tùng tối đa 150 ký tự.");
            if (p.DonGia < 0) throw new ArgumentException("Đơn giá không được âm.");
            if (p.SoLuongTon < 0) throw new ArgumentException("Số lượng tồn không được âm.");
        }

        public void Them(PhuTungDTO p) { Validate(p); dal.Insert(p); }
        public void Sua(PhuTungDTO p)
        {
            if (p.MaPhuTung <= 0) throw new ArgumentException("Chưa chọn phụ tùng cần sửa.");
            Validate(p); dal.Update(p);
        }
        public void Xoa(int ma)
        {
            if (ma <= 0) throw new ArgumentException("Chưa chọn phụ tùng cần xóa.");
            dal.Delete(ma);
        }
    }
}
