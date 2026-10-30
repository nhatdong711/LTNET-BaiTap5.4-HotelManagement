namespace HotelManagement.DTO
{
    /// <summary>Dữ liệu dịch vụ (bảng DichVu) – đóng vai trò ServiceDTO.</summary>
    public class DichVuDTO
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public decimal DonGia { get; set; }

        public override string ToString()
        {
            return TenDV + " - " + DonGia.ToString("N0") + " đ";
        }
    }
}
