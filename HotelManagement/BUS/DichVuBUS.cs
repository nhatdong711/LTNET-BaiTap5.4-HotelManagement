using System;
using System.Collections.Generic;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS
{
    public class DichVuBUS
    {
        private readonly DichVuDAL dal = new DichVuDAL();

        public List<DichVuDTO> GetAll() { return dal.GetAll(); }

        public bool Them(DichVuDTO dv) { KiemTra(dv); return dal.Insert(dv); }

        public bool Sua(DichVuDTO dv)
        {
            if (dv.MaDV <= 0) throw new ArgumentException("Vui lòng chọn dịch vụ cần sửa.");
            KiemTra(dv);
            return dal.Update(dv);
        }

        public bool Xoa(int maDV)
        {
            if (maDV <= 0) throw new ArgumentException("Vui lòng chọn dịch vụ cần xóa.");
            if (dal.CountSuDung(maDV) > 0) throw new InvalidOperationException("Dịch vụ đã được sử dụng, không thể xóa.");
            return dal.Delete(maDV);
        }

        private static void KiemTra(DichVuDTO dv)
        {
            if (string.IsNullOrWhiteSpace(dv.TenDV)) throw new ArgumentException("Tên dịch vụ không được để trống.");
            dv.TenDV = dv.TenDV.Trim();
            if (dv.DonGia < 0) throw new ArgumentException("Đơn giá không được âm.");
        }
    }
}
