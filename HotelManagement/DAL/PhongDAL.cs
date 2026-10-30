using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL
{
    public class PhongDAL
    {
        /// <summary>Nạp toàn bộ danh sách phòng vào DataTable để lọc ngắt kết nối bằng DataView.</summary>
        public DataTable GetAllTable()
        {
            return DatabaseHelper.ExecuteQuery(
                @"SELECT SoPhong, LoaiPhong, GiaTheoGio, TrangThai FROM Phong ORDER BY SoPhong");
        }

        public List<PhongDTO> GetTheoTrangThai(string trangThai)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT SoPhong, LoaiPhong, GiaTheoGio, TrangThai FROM Phong WHERE TrangThai = @TT ORDER BY SoPhong",
                new SqlParameter("@TT", trangThai));
            List<PhongDTO> list = new List<PhongDTO>();
            foreach (DataRow r in dt.Rows) list.Add(Map(r));
            return list;
        }

        public PhongDTO GetBySoPhong(string soPhong)
        {
            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT SoPhong, LoaiPhong, GiaTheoGio, TrangThai FROM Phong WHERE SoPhong = @So",
                new SqlParameter("@So", soPhong));
            return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
        }

        public bool Insert(PhongDTO p)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Phong (SoPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES (@So, @Loai, @Gia, @TT)",
                new SqlParameter("@So", p.SoPhong),
                new SqlParameter("@Loai", p.LoaiPhong),
                new SqlParameter("@Gia", p.GiaTheoGio),
                new SqlParameter("@TT", p.TrangThai)) > 0;
        }

        public bool Update(PhongDTO p)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Phong SET LoaiPhong = @Loai, GiaTheoGio = @Gia WHERE SoPhong = @So",
                new SqlParameter("@So", p.SoPhong),
                new SqlParameter("@Loai", p.LoaiPhong),
                new SqlParameter("@Gia", p.GiaTheoGio)) > 0;
        }

        public bool Delete(string soPhong)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM Phong WHERE SoPhong = @So", new SqlParameter("@So", soPhong)) > 0;
        }

        public int CountPhieu(string soPhong)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM PhieuDatPhong WHERE SoPhong = @So", new SqlParameter("@So", soPhong));
        }

        /// <summary>Phòng dọn xong: "Đang dọn dẹp" → "Trống".</summary>
        public bool DonXong(string soPhong)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE Phong SET TrangThai = @Trong WHERE SoPhong = @So AND TrangThai = @Don",
                new SqlParameter("@Trong", PhongDTO.TRONG),
                new SqlParameter("@Don", PhongDTO.DANG_DON),
                new SqlParameter("@So", soPhong)) > 0;
        }

        private static PhongDTO Map(DataRow r)
        {
            return new PhongDTO
            {
                SoPhong = r["SoPhong"].ToString(),
                LoaiPhong = r["LoaiPhong"].ToString(),
                GiaTheoGio = (decimal)r["GiaTheoGio"],
                TrangThai = r["TrangThai"].ToString()
            };
        }
    }
}
