namespace GarageManager.DAL
{
    public class KhachHangDAL
    {
        /// <summary>Xóa chủ xe (lỗi khóa ngoại nếu chủ xe còn xe).</summary>
        public int Delete(int maKH)
            => DbHelper.ExecuteNonQuery("DELETE FROM KhachHang WHERE MaKH = @Ma", DbHelper.P("@Ma", maKH));
    }
}
