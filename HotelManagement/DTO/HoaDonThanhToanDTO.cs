namespace HotelManagement.DTO
{
    /// <summary>Kết quả tính tiền khi check-out (không lưu thành bảng riêng).</summary>
    public class HoaDonThanhToanDTO
    {
        public int MaPhieu { get; set; }
        public string SoPhong { get; set; }
        public string HoTenKhach { get; set; }
        public double SoGio { get; set; }
        public decimal GiaTheoGio { get; set; }
        public decimal TienPhong { get; set; }
        public decimal TienDichVu { get; set; }
        public decimal TongTien { get { return TienPhong + TienDichVu; } }
    }
}
