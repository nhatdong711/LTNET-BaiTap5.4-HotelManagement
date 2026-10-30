using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL
{
    public class DichVuDAL
    {
        public List<DichVuDTO> GetAll()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT MaDV, TenDV, DonGia FROM DichVu ORDER BY MaDV");
            List<DichVuDTO> list = new List<DichVuDTO>();
            foreach (DataRow r in dt.Rows)
                list.Add(new DichVuDTO { MaDV = (int)r["MaDV"], TenDV = r["TenDV"].ToString(), DonGia = (decimal)r["DonGia"] });
            return list;
        }

        public bool Insert(DichVuDTO dv)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO DichVu (TenDV, DonGia) VALUES (@Ten, @Gia)",
                new SqlParameter("@Ten", dv.TenDV), new SqlParameter("@Gia", dv.DonGia)) > 0;
        }

        public bool Update(DichVuDTO dv)
        {
            return DatabaseHelper.ExecuteNonQuery(
                "UPDATE DichVu SET TenDV = @Ten, DonGia = @Gia WHERE MaDV = @Ma",
                new SqlParameter("@Ten", dv.TenDV), new SqlParameter("@Gia", dv.DonGia),
                new SqlParameter("@Ma", dv.MaDV)) > 0;
        }

        public bool Delete(int maDV)
        {
            return DatabaseHelper.ExecuteNonQuery("DELETE FROM DichVu WHERE MaDV = @Ma", new SqlParameter("@Ma", maDV)) > 0;
        }

        public int CountSuDung(int maDV)
        {
            return (int)DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM SuDungDichVu WHERE MaDV = @Ma", new SqlParameter("@Ma", maDV));
        }
    }
}
