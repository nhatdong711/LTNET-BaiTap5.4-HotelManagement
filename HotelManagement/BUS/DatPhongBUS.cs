using System;
using System.Data;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS
{
    /// <summary>Nghiệp vụ check-in, gọi dịch vụ, check-out.</summary>
    public class DatPhongBUS
    {
        private readonly PhieuDatPhongDAL dal = new PhieuDatPhongDAL();
        private readonly KhachHangDAL khDAL = new KhachHangDAL();

        public int CheckIn(KhachHangDTO kh, PhongDTO phong)
        {
            if (phong == null) throw new ArgumentException("Vui lòng chọn phòng còn trống.");
            if (kh.MaKH <= 0)
            {
                KhachHangBUS.KiemTraKhachMoi(kh);
                KhachHangDTO daCo = khDAL.GetByCCCD(kh.CCCD);
                if (daCo != null) kh.MaKH = daCo.MaKH; // CCCD đã có thì dùng lại khách cũ
            }
            return dal.CheckIn(kh, phong.SoPhong);
        }

        public PhieuDatPhongDTO GetPhieuDangO(string soPhong)
        {
            return dal.GetPhieuDangO(soPhong);
        }

        public void GoiDichVu(int maPhieu, DichVuDTO dv, int soLuong)
        {
            if (maPhieu <= 0) throw new ArgumentException("Phòng chưa có khách, không thể gọi dịch vụ.");
            if (dv == null) throw new ArgumentException("Vui lòng chọn dịch vụ.");
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            dal.ThemDichVu(new SuDungDichVuDTO { MaPhieu = maPhieu, MaDV = dv.MaDV, SoLuong = soLuong, ThoiGian = DateTime.Now });
        }

        public DataTable GetDichVuDaDung(int maPhieu) { return dal.GetDichVuDaDung(maPhieu); }

        /// <summary>
        /// Tính tiền: (thời gian ở × giá theo giờ) + tổng tiền dịch vụ.
        /// Thời gian ở làm tròn lên theo giờ, tối thiểu 1 giờ.
        /// </summary>
        public HoaDonThanhToanDTO TinhTien(PhieuDatPhongDTO phieu, DateTime thoiDiemTra)
        {
            if (phieu == null) throw new ArgumentException("Phòng chưa có khách.");
            double soGio = Math.Ceiling((thoiDiemTra - phieu.NgayNhanPhong).TotalHours);
            if (soGio < 1) soGio = 1;
            return new HoaDonThanhToanDTO
            {
                MaPhieu = phieu.MaPhieu,
                SoPhong = phieu.SoPhong,
                HoTenKhach = phieu.HoTenKhach,
                SoGio = soGio,
                GiaTheoGio = phieu.GiaTheoGio,
                TienPhong = (decimal)soGio * phieu.GiaTheoGio,
                TienDichVu = dal.GetTongTienDichVu(phieu.MaPhieu)
            };
        }

        public void CheckOut(HoaDonThanhToanDTO hd, DateTime thoiDiemTra)
        {
            if (hd == null) throw new ArgumentException("Chưa tính tiền.");
            dal.CheckOut(hd.MaPhieu, hd.SoPhong, thoiDiemTra, hd.TongTien);
        }

        public DataTable GetLichSu(DateTime tu, DateTime den) { KiemTraNgay(tu, den); return dal.GetLichSu(tu, den); }
        public decimal GetDoanhThu(DateTime tu, DateTime den) { KiemTraNgay(tu, den); return dal.GetDoanhThu(tu, den); }
        public DataTable GetDoanhThuTheoLoaiPhong(DateTime tu, DateTime den) { KiemTraNgay(tu, den); return dal.GetDoanhThuTheoLoaiPhong(tu, den); }

        private static void KiemTraNgay(DateTime tu, DateTime den)
        {
            if (tu.Date > den.Date) throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
        }
    }
}
