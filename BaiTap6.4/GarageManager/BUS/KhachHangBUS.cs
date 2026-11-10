using System;
using GarageManager.DAL;

namespace GarageManager.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL dal = new KhachHangDAL();

        public void Xoa(int maKH)
        {
            if (maKH <= 0) throw new ArgumentException("Chưa chọn chủ xe cần xóa.");
            dal.Delete(maKH);
        }
    }
}
