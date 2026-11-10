using System;

namespace GarageManager.DTO
{
    public class PhieuSuaChuaDTO
    {
        public int MaPhieu { get; set; }
        public string BienSo { get; set; }
        public DateTime NgayNhan { get; set; }
        public string NoiDungSua { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        // Hiển thị (JOIN)
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public string HieuXe { get; set; }
    }

    public static class TrangThaiPhieu
    {
        public const string DangSua = "Đang sửa";
        public const string DaHoanThanh = "Đã hoàn thành";
        public const string DaThanhToan = "Đã thanh toán";
        public static readonly string[] All = { DangSua, DaHoanThanh, DaThanhToan };
    }
}
