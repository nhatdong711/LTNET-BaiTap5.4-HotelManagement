using System;

namespace HotelManagement.DTO
{
    /// <summary>Dữ liệu phiếu đặt phòng (bảng PhieuDatPhong).</summary>
    public class PhieuDatPhongDTO
    {
        public int MaPhieu { get; set; }
        public string SoPhong { get; set; }
        public int MaKH { get; set; }
        public string HoTenKhach { get; set; }
        public DateTime NgayNhanPhong { get; set; }
        public DateTime? NgayTraPhong { get; set; }
        public decimal TongTienThanhToan { get; set; }
        public decimal GiaTheoGio { get; set; }
    }
}
