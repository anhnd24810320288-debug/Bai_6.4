using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;

namespace GarageManager.DAL
{
    public class XeDAL
    {
        private const string SelectBase = @"
SELECT x.BienSo, x.MaKH, x.HieuXe, x.NamSanXuat, kh.TenKH, kh.DienThoai, kh.DiaChi
FROM Xe x JOIN KhachHang kh ON kh.MaKH = x.MaKH ";

        private static string Str(SqlDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetString(i);
        }

        private static XeDTO Map(SqlDataReader r) => new XeDTO
        {
            BienSo = Str(r, "BienSo"),
            MaKH = r.GetInt32(r.GetOrdinal("MaKH")),
            HieuXe = Str(r, "HieuXe"),
            NamSanXuat = r.IsDBNull(r.GetOrdinal("NamSanXuat")) ? (int?)null : r.GetInt32(r.GetOrdinal("NamSanXuat")),
            TenKH = Str(r, "TenKH"),
            DienThoai = Str(r, "DienThoai"),
            DiaChi = Str(r, "DiaChi")
        };

        /// <summary>Tra cứu biển số bằng SqlParameter. Trả về null nếu là xe mới.</summary>
        public XeDTO GetByBienSo(string bienSo)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(SelectBase + "WHERE x.BienSo = @BienSo", conn))
            {
                cmd.Parameters.Add(new SqlParameter("@BienSo", bienSo));
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        public List<XeDTO> GetAll()
        {
            var list = new List<XeDTO>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(SelectBase + "ORDER BY x.BienSo", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>Tiếp nhận xe mới: tạo chủ xe (hoặc dùng lại chủ xe đã có cùng SĐT) rồi tạo xe — 1 transaction.</summary>
        public void TiepNhanXeMoi(KhachHangDTO kh, XeDTO xe)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int maKH = kh.MaKH;
                        if (maKH <= 0)
                        {
                            using (var find = new SqlCommand("SELECT MaKH FROM KhachHang WHERE DienThoai = @DT", conn, trans))
                            {
                                find.Parameters.AddWithValue("@DT", kh.DienThoai);
                                object found = find.ExecuteScalar();
                                if (found != null) maKH = Convert.ToInt32(found);
                            }
                        }
                        if (maKH <= 0)
                        {
                            using (var ins = new SqlCommand(
                                @"INSERT INTO KhachHang (TenKH, DienThoai, DiaChi) VALUES (@Ten, @DT, @DC);
                                  SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans))
                            {
                                ins.Parameters.Add(DbHelper.P("@Ten", kh.TenKH));
                                ins.Parameters.Add(DbHelper.P("@DT", kh.DienThoai));
                                ins.Parameters.Add(DbHelper.P("@DC", kh.DiaChi));
                                maKH = Convert.ToInt32(ins.ExecuteScalar());
                            }
                        }
                        using (var insXe = new SqlCommand(
                            "INSERT INTO Xe (BienSo, MaKH, HieuXe, NamSanXuat) VALUES (@BS, @MaKH, @Hieu, @Nam)", conn, trans))
                        {
                            insXe.Parameters.Add(DbHelper.P("@BS", xe.BienSo));
                            insXe.Parameters.Add(DbHelper.P("@MaKH", maKH));
                            insXe.Parameters.Add(DbHelper.P("@Hieu", xe.HieuXe));
                            insXe.Parameters.Add(DbHelper.P("@Nam", xe.NamSanXuat));
                            insXe.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                    catch { try { trans.Rollback(); } catch { } throw; }
                }
            }
        }

        /// <summary>Cập nhật thông tin xe và chủ xe (1 transaction).</summary>
        public void CapNhat(KhachHangDTO kh, XeDTO xe)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var c1 = new SqlCommand(
                            "UPDATE KhachHang SET TenKH=@Ten, DienThoai=@DT, DiaChi=@DC WHERE MaKH=@Ma", conn, trans))
                        {
                            c1.Parameters.Add(DbHelper.P("@Ten", kh.TenKH));
                            c1.Parameters.Add(DbHelper.P("@DT", kh.DienThoai));
                            c1.Parameters.Add(DbHelper.P("@DC", kh.DiaChi));
                            c1.Parameters.Add(DbHelper.P("@Ma", kh.MaKH));
                            c1.ExecuteNonQuery();
                        }
                        using (var c2 = new SqlCommand(
                            "UPDATE Xe SET HieuXe=@Hieu, NamSanXuat=@Nam WHERE BienSo=@BS", conn, trans))
                        {
                            c2.Parameters.Add(DbHelper.P("@Hieu", xe.HieuXe));
                            c2.Parameters.Add(DbHelper.P("@Nam", xe.NamSanXuat));
                            c2.Parameters.Add(DbHelper.P("@BS", xe.BienSo));
                            c2.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                    catch { try { trans.Rollback(); } catch { } throw; }
                }
            }
        }

        public int Xoa(string bienSo)
            => DbHelper.ExecuteNonQuery("DELETE FROM Xe WHERE BienSo = @BS", DbHelper.P("@BS", bienSo));
    }
}
