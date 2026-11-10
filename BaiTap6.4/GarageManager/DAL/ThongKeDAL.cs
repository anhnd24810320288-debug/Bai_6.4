using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;

namespace GarageManager.DAL
{
    public class ThongKeDAL
    {
        /// <summary>Doanh thu các phiếu đã thanh toán theo tháng (GROUP BY).</summary>
        public List<DoanhThuThangDTO> DoanhThuTheoThang()
        {
            const string sql = @"
SELECT CONVERT(VARCHAR(7), NgayNhan, 120) AS Thang, COUNT(*) AS SoPhieu, SUM(TongTien) AS DoanhThu
FROM PhieuSuaChua
WHERE TrangThai = N'Đã thanh toán'
GROUP BY CONVERT(VARCHAR(7), NgayNhan, 120)
ORDER BY Thang DESC";
            var list = new List<DoanhThuThangDTO>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new DoanhThuThangDTO { Thang = r.GetString(0), SoPhieu = r.GetInt32(1), DoanhThu = r.GetDecimal(2) });
            }
            return list;
        }

        /// <summary>Tổng doanh thu đã thanh toán (ExecuteScalar).</summary>
        public decimal TongDoanhThu()
            => Convert.ToDecimal(DbHelper.ExecuteScalar(
                "SELECT ISNULL(SUM(TongTien), 0) FROM PhieuSuaChua WHERE TrangThai = N'Đã thanh toán'"));
    }
}
