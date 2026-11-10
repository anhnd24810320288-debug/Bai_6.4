using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;

namespace GarageManager.DAL
{
    public class PhuTungDAL
    {
        public List<PhuTungDTO> GetAll()
        {
            var list = new List<PhuTungDTO>();
            const string sql = "SELECT MaPhuTung, TenPhuTung, DonGia, SoLuongTon FROM PhuTung ORDER BY TenPhuTung";
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new PhuTungDTO
                        {
                            MaPhuTung = r.GetInt32(0), TenPhuTung = r.GetString(1),
                            DonGia = r.GetDecimal(2), SoLuongTon = r.GetInt32(3)
                        });
            }
            return list;
        }

        public int Insert(PhuTungDTO p)
            => DbHelper.ExecuteNonQuery(
                "INSERT INTO PhuTung (TenPhuTung, DonGia, SoLuongTon) VALUES (@Ten, @Gia, @SL)",
                DbHelper.P("@Ten", p.TenPhuTung), DbHelper.P("@Gia", p.DonGia), DbHelper.P("@SL", p.SoLuongTon));

        /// <summary>Cập nhật đơn giá và số lượng tồn kho.</summary>
        public int Update(PhuTungDTO p)
            => DbHelper.ExecuteNonQuery(
                "UPDATE PhuTung SET TenPhuTung=@Ten, DonGia=@Gia, SoLuongTon=@SL WHERE MaPhuTung=@Ma",
                DbHelper.P("@Ten", p.TenPhuTung), DbHelper.P("@Gia", p.DonGia),
                DbHelper.P("@SL", p.SoLuongTon), DbHelper.P("@Ma", p.MaPhuTung));

        public int Delete(int ma)
            => DbHelper.ExecuteNonQuery("DELETE FROM PhuTung WHERE MaPhuTung=@Ma", DbHelper.P("@Ma", ma));

        public List<PhuTungDTO> GetTonThap(int nguong)
        {
            var list = new List<PhuTungDTO>();
            const string sql = @"SELECT MaPhuTung, TenPhuTung, DonGia, SoLuongTon FROM PhuTung
                                 WHERE SoLuongTon <= @N ORDER BY SoLuongTon, TenPhuTung";
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@N", nguong);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new PhuTungDTO
                        {
                            MaPhuTung = r.GetInt32(0), TenPhuTung = r.GetString(1),
                            DonGia = r.GetDecimal(2), SoLuongTon = r.GetInt32(3)
                        });
            }
            return list;
        }
    }
}
