namespace GarageManager.DTO
{
    public class ChiTietSuaChuaDTO
    {
        public int MaPhieu { get; set; }
        public int MaPhuTung { get; set; }
        public string TenPhuTung { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal TienCong { get; set; }

        /// <summary>Số lượng × đơn giá phụ tùng.</summary>
        public decimal TienPhuTung => SoLuong * DonGia;

        /// <summary>Thành tiền dòng = tiền phụ tùng + tiền công.</summary>
        public decimal ThanhTien => SoLuong * DonGia + TienCong;
    }
}
