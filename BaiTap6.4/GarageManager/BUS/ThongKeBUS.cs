using System.Collections.Generic;
using GarageManager.DAL;
using GarageManager.DTO;

namespace GarageManager.BUS
{
    public class ThongKeBUS
    {
        private readonly ThongKeDAL dal = new ThongKeDAL();
        public List<DoanhThuThangDTO> DoanhThuTheoThang() => dal.DoanhThuTheoThang();
        public decimal TongDoanhThu() => dal.TongDoanhThu();
    }
}
