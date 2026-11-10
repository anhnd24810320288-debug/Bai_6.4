namespace GarageManager.DTO
{
    public class XeDTO
    {
        public string BienSo { get; set; }
        public int MaKH { get; set; }
        public string HieuXe { get; set; }
        public int? NamSanXuat { get; set; }

        // Thông tin chủ xe (JOIN) - dùng để hiển thị
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public string DiaChi { get; set; }
    }
}
