using System;
using System.Data;
using System.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL
{
    public class PhieuDatPhongDAL
    {
        /// <summary>
        /// Check-in trong 1 SqlTransaction: (thêm khách mới nếu chưa có) → lập phiếu → phòng sang "Đang ở".
        /// Trả về mã phiếu vừa tạo.
        /// </summary>
        public int CheckIn(KhachHangDTO kh, string soPhong)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Khóa dòng phòng và kiểm tra còn trống (tránh 2 lễ tân cùng nhận 1 phòng)
                    SqlCommand cmdKT = new SqlCommand(
                        "SELECT TrangThai FROM Phong WITH (UPDLOCK, ROWLOCK) WHERE SoPhong = @So", conn, trans);
                    cmdKT.Parameters.AddWithValue("@So", soPhong);
                    object tt = cmdKT.ExecuteScalar();
                    if (tt == null || tt.ToString() != PhongDTO.TRONG)
                        throw new InvalidOperationException("Phòng " + soPhong + " không còn trống.");

                    int maKH = kh.MaKH;
                    if (maKH <= 0)
                    {
                        SqlCommand cmdKH = new SqlCommand(
                            @"INSERT INTO KhachHang (HoTen, CCCD, SoDienThoai) VALUES (@Ten, @CCCD, @SDT);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans);
                        cmdKH.Parameters.AddWithValue("@Ten", kh.HoTen);
                        cmdKH.Parameters.AddWithValue("@CCCD", kh.CCCD);
                        cmdKH.Parameters.AddWithValue("@SDT", string.IsNullOrEmpty(kh.SoDienThoai) ? (object)DBNull.Value : kh.SoDienThoai);
                        maKH = (int)cmdKH.ExecuteScalar();
                    }

                    SqlCommand cmdPhieu = new SqlCommand(
                        @"INSERT INTO PhieuDatPhong (SoPhong, MaKH, NgayNhanPhong) VALUES (@So, @MaKH, GETDATE());
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, trans);
                    cmdPhieu.Parameters.AddWithValue("@So", soPhong);
                    cmdPhieu.Parameters.AddWithValue("@MaKH", maKH);
                    int maPhieu = (int)cmdPhieu.ExecuteScalar();

                    SqlCommand cmdPhong = new SqlCommand("UPDATE Phong SET TrangThai = @TT WHERE SoPhong = @So", conn, trans);
                    cmdPhong.Parameters.AddWithValue("@TT", PhongDTO.DANG_O);
                    cmdPhong.Parameters.AddWithValue("@So", soPhong);
                    cmdPhong.ExecuteNonQuery();

                    trans.Commit();
                    return maPhieu;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Lấy phiếu đang mở (chưa trả phòng) của một phòng.</summary>
        public PhieuDatPhongDTO GetPhieuDangO(string soPhong)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                @"SELECT TOP 1 p.MaPhieu, p.SoPhong, p.MaKH, k.HoTen, p.NgayNhanPhong, ph.GiaTheoGio
                  FROM PhieuDatPhong p
                  JOIN KhachHang k ON p.MaKH = k.MaKH
                  JOIN Phong ph ON p.SoPhong = ph.SoPhong
                  WHERE p.SoPhong = @So AND p.NgayTraPhong IS NULL
                  ORDER BY p.MaPhieu DESC",
                new SqlParameter("@So", soPhong));
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new PhieuDatPhongDTO
            {
                MaPhieu = (int)r["MaPhieu"],
                SoPhong = r["SoPhong"].ToString(),
                MaKH = (int)r["MaKH"],
                HoTenKhach = r["HoTen"].ToString(),
                NgayNhanPhong = (DateTime)r["NgayNhanPhong"],
                GiaTheoGio = (decimal)r["GiaTheoGio"]
            };
        }

        /// <summary>Ghi nhận phòng gọi dịch vụ (dữ liệu truyền qua SuDungDichVuDTO).</summary>
        public bool ThemDichVu(SuDungDichVuDTO sd)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO SuDungDichVu (MaPhieu, MaDV, SoLuong, ThoiGian) VALUES (@MaPhieu, @MaDV, @SL, @TG)",
                new SqlParameter("@MaPhieu", sd.MaPhieu),
                new SqlParameter("@MaDV", sd.MaDV),
                new SqlParameter("@SL", sd.SoLuong),
                new SqlParameter("@TG", sd.ThoiGian)) > 0;
        }

        public DataTable GetDichVuDaDung(int maPhieu)
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT d.TenDV AS [Dịch vụ], s.SoLuong AS [SL], d.DonGia AS [Đơn giá],
                         s.SoLuong * d.DonGia AS [Thành tiền], s.ThoiGian AS [Thời gian]
                  FROM SuDungDichVu s JOIN DichVu d ON s.MaDV = d.MaDV
                  WHERE s.MaPhieu = @MaPhieu
                  ORDER BY s.ThoiGian",
                new SqlParameter("@MaPhieu", maPhieu));
        }

        public decimal GetTongTienDichVu(int maPhieu)
        {
            object kq = DatabaseHelper.ExecuteScalar(
                @"SELECT ISNULL(SUM(s.SoLuong * d.DonGia), 0)
                  FROM SuDungDichVu s JOIN DichVu d ON s.MaDV = d.MaDV WHERE s.MaPhieu = @MaPhieu",
                new SqlParameter("@MaPhieu", maPhieu));
            return Convert.ToDecimal(kq);
        }

        /// <summary>
        /// Check-out trong 1 SqlTransaction: lưu TongTienThanhToan + NgayTraPhong → phòng sang "Đang dọn dẹp".
        /// </summary>
        public void CheckOut(int maPhieu, string soPhong, DateTime ngayTra, decimal tongTien)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdPhieu = new SqlCommand(
                        @"UPDATE PhieuDatPhong SET NgayTraPhong = @NgayTra, TongTienThanhToan = @Tong
                          WHERE MaPhieu = @MaPhieu AND NgayTraPhong IS NULL", conn, trans);
                    cmdPhieu.Parameters.AddWithValue("@NgayTra", ngayTra);
                    cmdPhieu.Parameters.AddWithValue("@Tong", tongTien);
                    cmdPhieu.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    if (cmdPhieu.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Phiếu đã được thanh toán trước đó.");

                    SqlCommand cmdPhong = new SqlCommand("UPDATE Phong SET TrangThai = @TT WHERE SoPhong = @So", conn, trans);
                    cmdPhong.Parameters.AddWithValue("@TT", PhongDTO.DANG_DON);
                    cmdPhong.Parameters.AddWithValue("@So", soPhong);
                    cmdPhong.ExecuteNonQuery();

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public DataTable GetLichSu(DateTime tu, DateTime den)
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT p.MaPhieu AS [Mã phiếu], p.SoPhong AS [Phòng], k.HoTen AS [Khách hàng], k.CCCD,
                         p.NgayNhanPhong AS [Nhận phòng], p.NgayTraPhong AS [Trả phòng],
                         p.TongTienThanhToan AS [Tổng tiền]
                  FROM PhieuDatPhong p JOIN KhachHang k ON p.MaKH = k.MaKH
                  WHERE p.NgayTraPhong >= @Tu AND p.NgayTraPhong < @Den
                  ORDER BY p.NgayTraPhong DESC",
                new SqlParameter("@Tu", tu.Date), new SqlParameter("@Den", den.Date.AddDays(1)));
        }

        public decimal GetDoanhThu(DateTime tu, DateTime den)
        {
            return Convert.ToDecimal(DatabaseHelper.ExecuteScalar(
                @"SELECT ISNULL(SUM(TongTienThanhToan), 0) FROM PhieuDatPhong
                  WHERE NgayTraPhong >= @Tu AND NgayTraPhong < @Den",
                new SqlParameter("@Tu", tu.Date), new SqlParameter("@Den", den.Date.AddDays(1))));
        }

        public DataTable GetDoanhThuTheoLoaiPhong(DateTime tu, DateTime den)
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT ph.LoaiPhong AS [Loại phòng], COUNT(*) AS [Số lượt], SUM(p.TongTienThanhToan) AS [Doanh thu]
                  FROM PhieuDatPhong p JOIN Phong ph ON p.SoPhong = ph.SoPhong
                  WHERE p.NgayTraPhong >= @Tu AND p.NgayTraPhong < @Den
                  GROUP BY ph.LoaiPhong ORDER BY SUM(p.TongTienThanhToan) DESC",
                new SqlParameter("@Tu", tu.Date), new SqlParameter("@Den", den.Date.AddDays(1)));
        }
    }
}
