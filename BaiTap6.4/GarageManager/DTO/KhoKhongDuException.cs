using System;

namespace GarageManager.DTO
{
    /// <summary>Ném ra khi phụ tùng trong kho không đủ số lượng → giao dịch bị Rollback.</summary>
    public class KhoKhongDuException : Exception
    {
        public string TenPhuTung { get; }
        public int SoLuongTon { get; }
        public int SoLuongCan { get; }

        public KhoKhongDuException(string ten, int ton, int can)
            : base($"Phụ tùng \"{ten}\" không đủ tồn kho (cần {can}, chỉ còn {ton}).")
        {
            TenPhuTung = ten; SoLuongTon = ton; SoLuongCan = can;
        }
    }
}
