using System;
using System.Data;
using System.Linq;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL dal = new KhachHangDAL();

        public KhachHangDTO TraCuuTheoCCCD(string cccd)
        {
            cccd = KiemTraCCCD(cccd);
            return dal.GetByCCCD(cccd);
        }

        public DataTable GetAll() { return dal.GetAll(); }

        public static string KiemTraCCCD(string cccd)
        {
            cccd = (cccd ?? "").Trim();
            if (cccd.Length == 0) throw new ArgumentException("Vui lòng nhập số CCCD.");
            if (!cccd.All(char.IsDigit) || (cccd.Length != 9 && cccd.Length != 12))
                throw new ArgumentException("CCCD phải gồm 12 chữ số (hoặc CMND 9 số).");
            return cccd;
        }

        public static void KiemTraKhachMoi(KhachHangDTO kh)
        {
            kh.CCCD = KiemTraCCCD(kh.CCCD);
            if (string.IsNullOrWhiteSpace(kh.HoTen)) throw new ArgumentException("Họ tên khách không được để trống.");
            kh.HoTen = kh.HoTen.Trim();
            kh.SoDienThoai = (kh.SoDienThoai ?? "").Trim();
            if (kh.SoDienThoai.Length > 0 && (!kh.SoDienThoai.All(char.IsDigit) || kh.SoDienThoai.Length < 9 || kh.SoDienThoai.Length > 15))
                throw new ArgumentException("Số điện thoại không hợp lệ.");
        }
    }
}
