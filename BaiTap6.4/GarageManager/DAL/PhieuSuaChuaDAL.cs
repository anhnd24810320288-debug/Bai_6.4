using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;

namespace GarageManager.DAL
{
    public class PhieuSuaChuaDAL
    {
        /// <summary>
        /// Lập phiếu sửa chữa trong MỘT SqlTransaction:
        /// 1) tạo PhieuSuaChua  2) tạo ChiTietSuaChua  3) trừ SoLuongTon trong PhuTung
        /// 4) nếu thiếu phụ tùng → Rollback toàn bộ và ném KhoKhongDuException.
        /// </summary>
        public int TaoPhieu(PhieuSuaChuaDTO p, List<ChiTietSuaChuaDTO> dsChiTiet)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Phiếu
                        int maPhieu;
                        using (var cmd = new SqlCommand(
                            @"INSERT INTO PhieuSuaChua (BienSo, NoiDungSua, TongTien, TrangThai)
                              VALUES (@BS, @ND, 0, @TT);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans))
                        {
                            cmd.Parameters.Add(DbHelper.P("@BS", p.BienSo));
                            cmd.Parameters.Add(DbHelper.P("@ND", p.NoiDungSua));
                            cmd.Parameters.Add(DbHelper.P("@TT", p.TrangThai));
                            maPhieu = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        decimal tongTien = 0;
                        foreach (var ct in dsChiTiet)
                        {
                            // Khóa dòng phụ tùng và đọc giá/tồn thực tế trong DB
                            string ten; decimal gia; int ton;
                            using (var sel = new SqlCommand(
                                @"SELECT TenPhuTung, DonGia, SoLuongTon
                                  FROM PhuTung WITH (UPDLOCK, ROWLOCK) WHERE MaPhuTung = @Ma", conn, trans))
                            {
                                sel.Parameters.AddWithValue("@Ma", ct.MaPhuTung);
                                using (var r = sel.ExecuteReader())
                                {
                                    if (!r.Read())
                                        throw new InvalidOperationException("Phụ tùng mã " + ct.MaPhuTung + " không tồn tại.");
                                    ten = r.GetString(0); gia = r.GetDecimal(1); ton = r.GetInt32(2);
                                }
                            }

                            // 4) Không đủ hàng → ném lỗi (catch bên dưới sẽ Rollback)
                            if (ton < ct.SoLuong) throw new KhoKhongDuException(ten, ton, ct.SoLuong);

                            // 3) Trừ kho
                            using (var upd = new SqlCommand(
                                @"UPDATE PhuTung SET SoLuongTon = SoLuongTon - @SL
                                  WHERE MaPhuTung = @Ma AND SoLuongTon >= @SL", conn, trans))
                            {
                                upd.Parameters.AddWithValue("@SL", ct.SoLuong);
                                upd.Parameters.AddWithValue("@Ma", ct.MaPhuTung);
                                if (upd.ExecuteNonQuery() != 1) throw new KhoKhongDuException(ten, ton, ct.SoLuong);
                            }

                            // 2) Chi tiết
                            using (var insCt = new SqlCommand(
                                @"INSERT INTO ChiTietSuaChua (MaPhieu, MaPhuTung, SoLuong, TienCong)
                                  VALUES (@Phieu, @PT, @SL, @Cong)", conn, trans))
                            {
                                insCt.Parameters.AddWithValue("@Phieu", maPhieu);
                                insCt.Parameters.AddWithValue("@PT", ct.MaPhuTung);
                                insCt.Parameters.AddWithValue("@SL", ct.SoLuong);
                                insCt.Parameters.AddWithValue("@Cong", ct.TienCong);
                                insCt.ExecuteNonQuery();
                            }
                            tongTien += ct.SoLuong * gia + ct.TienCong;
                        }

                        using (var updTong = new SqlCommand(
                            "UPDATE PhieuSuaChua SET TongTien = @T WHERE MaPhieu = @M", conn, trans))
                        {
                            updTong.Parameters.AddWithValue("@T", tongTien);
                            updTong.Parameters.AddWithValue("@M", maPhieu);
                            updTong.ExecuteNonQuery();
                        }

                        trans.Commit();
                        return maPhieu;
                    }
                    catch
                    {
                        try { trans.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public List<PhieuSuaChuaDTO> GetAll(string trangThai)
        {
            const string sql = @"
SELECT p.MaPhieu, p.BienSo, p.NgayNhan, p.NoiDungSua, p.TongTien, p.TrangThai, kh.TenKH, kh.DienThoai, x.HieuXe
FROM PhieuSuaChua p
JOIN Xe x ON x.BienSo = p.BienSo
JOIN KhachHang kh ON kh.MaKH = x.MaKH
WHERE (@TT IS NULL OR p.TrangThai = @TT)
ORDER BY p.NgayNhan DESC, p.MaPhieu DESC";
            var list = new List<PhieuSuaChuaDTO>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(DbHelper.P("@TT", trangThai));
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new PhieuSuaChuaDTO
                        {
                            MaPhieu = r.GetInt32(0),
                            BienSo = r.GetString(1),
                            NgayNhan = r.GetDateTime(2),
                            NoiDungSua = r.IsDBNull(3) ? null : r.GetString(3),
                            TongTien = r.IsDBNull(4) ? 0 : r.GetDecimal(4),
                            TrangThai = r.GetString(5),
                            TenKH = r.GetString(6),
                            DienThoai = r.IsDBNull(7) ? null : r.GetString(7),
                            HieuXe = r.IsDBNull(8) ? null : r.GetString(8)
                        });
                    }
                }
            }
            return list;
        }

        public List<ChiTietSuaChuaDTO> GetChiTiet(int maPhieu)
        {
            const string sql = @"
SELECT ct.MaPhieu, ct.MaPhuTung, pt.TenPhuTung, ct.SoLuong, pt.DonGia, ct.TienCong
FROM ChiTietSuaChua ct JOIN PhuTung pt ON pt.MaPhuTung = ct.MaPhuTung
WHERE ct.MaPhieu = @Ma ORDER BY pt.TenPhuTung";
            var list = new List<ChiTietSuaChuaDTO>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Ma", maPhieu);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new ChiTietSuaChuaDTO
                        {
                            MaPhieu = r.GetInt32(0), MaPhuTung = r.GetInt32(1), TenPhuTung = r.GetString(2),
                            SoLuong = r.GetInt32(3), DonGia = r.GetDecimal(4), TienCong = r.GetDecimal(5)
                        });
            }
            return list;
        }

        /// <summary>
        /// Thanh toán: tính lại tổng = Σ(SoLuong×DonGia) + ΣTienCong và chuyển sang 'Đã thanh toán'.
        /// Chỉ áp dụng cho phiếu đang ở trạng thái 'Đã hoàn thành'. Trả về số dòng cập nhật.
        /// </summary>
        public int ThanhToan(int maPhieu)
        {
            const string sql = @"
UPDATE p
SET TrangThai = N'Đã thanh toán',
    TongTien = ISNULL((SELECT SUM(ct.SoLuong * pt.DonGia + ct.TienCong)
                       FROM ChiTietSuaChua ct JOIN PhuTung pt ON pt.MaPhuTung = ct.MaPhuTung
                       WHERE ct.MaPhieu = p.MaPhieu), 0)
FROM PhieuSuaChua p
WHERE p.MaPhieu = @Ma AND p.TrangThai = N'Đã hoàn thành'";
            return DbHelper.ExecuteNonQuery(sql, DbHelper.P("@Ma", maPhieu));
        }
    }
}
