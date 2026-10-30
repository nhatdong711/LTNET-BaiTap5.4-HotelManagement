using System.Data;
using System.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL
{
    public class KhachHangDAL
    {
        /// <summary>Tra cứu khách theo CCCD – dùng SqlParameter, không nối chuỗi.</summary>
        public KhachHangDTO GetByCCCD(string cccd)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT MaKH, HoTen, CCCD, SoDienThoai FROM KhachHang WHERE CCCD = @CCCD",
                new SqlParameter("@CCCD", SqlDbType.VarChar, 20) { Value = cccd });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new KhachHangDTO
            {
                MaKH = (int)r["MaKH"],
                HoTen = r["HoTen"].ToString(),
                CCCD = r["CCCD"].ToString(),
                SoDienThoai = r["SoDienThoai"].ToString()
            };
        }

        public DataTable GetAll()
        {
            return DatabaseHelper.ExecuteQuery(
                "SELECT MaKH AS [Mã KH], HoTen AS [Họ tên], CCCD, SoDienThoai AS [SĐT] FROM KhachHang ORDER BY MaKH DESC");
        }
    }
}
