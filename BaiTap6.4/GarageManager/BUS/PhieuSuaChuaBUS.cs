using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GarageManager.DAL;
using GarageManager.DTO;

namespace GarageManager.BUS
{
    public class PhieuSuaChuaBUS
    {
        private readonly PhieuSuaChuaDAL dal = new PhieuSuaChuaDAL();

        public List<PhieuSuaChuaDTO> GetAll(string trangThai) => dal.GetAll(trangThai);
        public List<ChiTietSuaChuaDTO> GetChiTiet(int maPhieu) => dal.GetChiTiet(maPhieu);

        /// <summary>Lập & xác nhận hoàn tất phiếu (SqlTransaction nằm ở DAL). Trả về mã phiếu.</summary>
        public int LapPhieu(string bienSo, string noiDung, List<ChiTietSuaChuaDTO> dong)
        {
            if (string.IsNullOrWhiteSpace(bienSo)) throw new ArgumentException("Chưa chọn xe cho phiếu sửa chữa.");
            if (dong == null || dong.Count == 0) throw new ArgumentException("Phiếu phải có ít nhất một phụ tùng/dịch vụ.");

            // Gộp các dòng trùng phụ tùng (khóa chính của ChiTietSuaChua là MaPhieu + MaPhuTung)
            var gop = dong.GroupBy(d => d.MaPhuTung)
                          .Select(g => new ChiTietSuaChuaDTO
                          {
                              MaPhuTung = g.Key,
                              SoLuong = g.Sum(x => x.SoLuong),
                              TienCong = g.Sum(x => x.TienCong)
                          }).ToList();
            foreach (var d in gop)
            {
                if (d.SoLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
                if (d.TienCong < 0) throw new ArgumentException("Tiền công không được âm.");
            }

            var phieu = new PhieuSuaChuaDTO
            {
                BienSo = XeBUS.ChuanHoaBienSo(bienSo),
                NoiDungSua = (noiDung ?? "").Trim(),
                TrangThai = TrangThaiPhieu.DaHoanThanh
            };
            return dal.TaoPhieu(phieu, gop);
        }

        public void ThanhToan(int maPhieu)
        {
            if (maPhieu <= 0) throw new ArgumentException("Chưa chọn phiếu cần thanh toán.");
            if (dal.ThanhToan(maPhieu) == 0)
                throw new InvalidOperationException("Chỉ thanh toán được phiếu ở trạng thái 'Đã hoàn thành'.");
        }

        /// <summary>Tạo nội dung hóa đơn dạng văn bản.</summary>
        public static string TaoHoaDon(PhieuSuaChuaDTO p, List<ChiTietSuaChuaDTO> ct)
        {
            decimal tienPT = ct.Sum(x => x.TienPhuTung);
            decimal tienCong = ct.Sum(x => x.TienCong);
            var sb = new StringBuilder();
            sb.AppendLine("==================== GARAGE AUTO ====================");
            sb.AppendLine("          HÓA ĐƠN SỬA CHỮA - BẢO DƯỠNG XE");
            sb.AppendLine("=====================================================");
            sb.AppendLine($"Số phiếu  : #{p.MaPhieu}");
            sb.AppendLine($"Ngày nhận : {p.NgayNhan:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Biển số   : {p.BienSo}   Hiệu xe: {p.HieuXe}");
            sb.AppendLine($"Chủ xe    : {p.TenKH}   SĐT: {p.DienThoai}");
            sb.AppendLine($"Nội dung  : {p.NoiDungSua}");
            sb.AppendLine("-----------------------------------------------------");
            sb.AppendLine("STT  Phụ tùng                    SL     Đơn giá    Tiền công   Thành tiền");
            int i = 1;
            foreach (var x in ct)
                sb.AppendLine($"{i++,-4} {Cut(x.TenPhuTung, 26),-26} {x.SoLuong,4} {x.DonGia,12:N0} {x.TienCong,12:N0} {x.ThanhTien,12:N0}");
            sb.AppendLine("-----------------------------------------------------");
            sb.AppendLine($"Tiền phụ tùng : {tienPT,15:N0} đ");
            sb.AppendLine($"Tiền công     : {tienCong,15:N0} đ");
            sb.AppendLine($"TỔNG CỘNG     : {tienPT + tienCong,15:N0} đ");
            sb.AppendLine($"Trạng thái    : {p.TrangThai}");
            sb.AppendLine("=====================================================");
            sb.AppendLine("            Cảm ơn quý khách đã sử dụng dịch vụ!");
            return sb.ToString();
        }

        private static string Cut(string s, int n) => string.IsNullOrEmpty(s) ? "" : (s.Length <= n ? s : s.Substring(0, n));
    }
}
