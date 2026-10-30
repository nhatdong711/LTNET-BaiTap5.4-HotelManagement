namespace HotelManagement.DTO
{
    /// <summary>Dữ liệu phòng khách sạn (bảng Phong).</summary>
    public class PhongDTO
    {
        public const string TRONG = "Trống";
        public const string DANG_O = "Đang ở";
        public const string DANG_DON = "Đang dọn dẹp";

        public string SoPhong { get; set; }
        public string LoaiPhong { get; set; }
        public decimal GiaTheoGio { get; set; }
        public string TrangThai { get; set; }

        public override string ToString()
        {
            return SoPhong + " - " + LoaiPhong + " (" + GiaTheoGio.ToString("N0") + " đ/giờ)";
        }
    }
}
