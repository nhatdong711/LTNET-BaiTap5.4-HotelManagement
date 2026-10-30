using System;

namespace HotelManagement.DTO
{
    /// <summary>Một lần phòng gọi dịch vụ (bảng SuDungDichVu).</summary>
    public class SuDungDichVuDTO
    {
        public int MaPhieu { get; set; }
        public int MaDV { get; set; }
        public int SoLuong { get; set; }
        public DateTime ThoiGian { get; set; }
    }
}
