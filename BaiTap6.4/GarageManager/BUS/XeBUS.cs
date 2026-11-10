using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GarageManager.DAL;
using GarageManager.DTO;

namespace GarageManager.BUS
{
    public class XeBUS
    {
        private readonly XeDAL dal = new XeDAL();

        public static string ChuanHoaBienSo(string s) => (s ?? "").Trim().ToUpper();

        public XeDTO TraCuu(string bienSo)
        {
            bienSo = ChuanHoaBienSo(bienSo);
            if (bienSo.Length == 0) throw new ArgumentException("Vui lòng nhập biển số xe.");
            return dal.GetByBienSo(bienSo);
        }

        public List<XeDTO> GetAll() => dal.GetAll();

        private static void Validate(KhachHangDTO kh, XeDTO xe)
        {
            xe.BienSo = ChuanHoaBienSo(xe.BienSo);
            if (xe.BienSo.Length == 0) throw new ArgumentException("Biển số không được để trống.");
            if (xe.BienSo.Length > 20) throw new ArgumentException("Biển số tối đa 20 ký tự.");
            if (xe.NamSanXuat.HasValue && (xe.NamSanXuat < 1950 || xe.NamSanXuat > DateTime.Now.Year + 1))
                throw new ArgumentException("Năm sản xuất không hợp lệ.");
            if (string.IsNullOrWhiteSpace(kh.TenKH)) throw new ArgumentException("Tên chủ xe không được để trống.");
            kh.DienThoai = (kh.DienThoai ?? "").Trim();
            if (!Regex.IsMatch(kh.DienThoai, @"^\+?[0-9 .\-]{8,15}$"))
                throw new ArgumentException("Số điện thoại không hợp lệ (8–15 chữ số).");
        }

        public void TiepNhanXeMoi(KhachHangDTO kh, XeDTO xe)
        {
            Validate(kh, xe);
            if (dal.GetByBienSo(xe.BienSo) != null)
                throw new ArgumentException("Biển số " + xe.BienSo + " đã có trong hệ thống. Hãy dùng chức năng Cập nhật.");
            dal.TiepNhanXeMoi(kh, xe);
        }

        public void CapNhat(KhachHangDTO kh, XeDTO xe)
        {
            if (kh.MaKH <= 0) throw new ArgumentException("Hãy tra cứu hoặc chọn một xe có sẵn để cập nhật.");
            Validate(kh, xe);
            dal.CapNhat(kh, xe);
        }

        public void Xoa(string bienSo)
        {
            bienSo = ChuanHoaBienSo(bienSo);
            if (bienSo.Length == 0) throw new ArgumentException("Chưa chọn xe cần xóa.");
            dal.Xoa(bienSo);
        }
    }
}
