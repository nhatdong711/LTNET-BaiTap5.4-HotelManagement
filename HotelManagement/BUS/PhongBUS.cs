using System;
using System.Collections.Generic;
using System.Data;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS
{
    public class PhongBUS
    {
        private readonly PhongDAL dal = new PhongDAL();

        public static readonly string[] LOAI_PHONG = { "Đơn", "Đôi", "VIP" };

        public DataTable GetAllTable() { return dal.GetAllTable(); }

        public List<PhongDTO> GetPhongTrong() { return dal.GetTheoTrangThai(PhongDTO.TRONG); }

        /// <summary>
        /// Tạo biểu thức RowFilter cho DataView (lọc ngắt kết nối, không truy vấn lại CSDL).
        /// Giá trị được escape dấu nháy đơn để biểu thức luôn hợp lệ.
        /// </summary>
        public static string TaoBoLoc(string loaiPhong, string trangThai, string tuKhoa)
        {
            List<string> dk = new List<string>();
            if (!string.IsNullOrEmpty(loaiPhong)) dk.Add("LoaiPhong = '" + Escape(loaiPhong) + "'");
            if (!string.IsNullOrEmpty(trangThai)) dk.Add("TrangThai = '" + Escape(trangThai) + "'");
            if (!string.IsNullOrWhiteSpace(tuKhoa)) dk.Add("SoPhong LIKE '%" + EscapeLike(tuKhoa.Trim()) + "%'");
            return string.Join(" AND ", dk);
        }

        private static string Escape(string s)
        {
            return s.Replace("'", "''");
        }

        private static string EscapeLike(string s)
        {
            // Bỏ các ký tự đại diện của LIKE trong DataView để từ khóa luôn được hiểu là chữ thường
            foreach (char c in new[] { '[', ']', '%', '*' }) s = s.Replace(c.ToString(), "");
            return Escape(s);
        }

        public bool Them(PhongDTO p)
        {
            KiemTra(p);
            if (dal.GetBySoPhong(p.SoPhong) != null) throw new ArgumentException("Số phòng " + p.SoPhong + " đã tồn tại.");
            p.TrangThai = PhongDTO.TRONG;
            return dal.Insert(p);
        }

        public bool Sua(PhongDTO p)
        {
            KiemTra(p);
            if (dal.GetBySoPhong(p.SoPhong) == null) throw new ArgumentException("Không tìm thấy phòng " + p.SoPhong + ".");
            return dal.Update(p);
        }

        public bool Xoa(string soPhong)
        {
            if (string.IsNullOrWhiteSpace(soPhong)) throw new ArgumentException("Vui lòng chọn phòng cần xóa.");
            if (dal.CountPhieu(soPhong) > 0) throw new InvalidOperationException("Phòng đã có lịch sử đặt phòng, không thể xóa.");
            return dal.Delete(soPhong);
        }

        public void DonXong(string soPhong)
        {
            if (!dal.DonXong(soPhong))
                throw new InvalidOperationException("Chỉ phòng ở trạng thái \"Đang dọn dẹp\" mới đánh dấu dọn xong được.");
        }

        private static void KiemTra(PhongDTO p)
        {
            if (string.IsNullOrWhiteSpace(p.SoPhong)) throw new ArgumentException("Số phòng không được để trống.");
            p.SoPhong = p.SoPhong.Trim().ToUpper();
            if (p.SoPhong.Length > 10) throw new ArgumentException("Số phòng tối đa 10 ký tự.");
            if (string.IsNullOrWhiteSpace(p.LoaiPhong)) throw new ArgumentException("Vui lòng chọn loại phòng.");
            if (p.GiaTheoGio <= 0) throw new ArgumentException("Giá theo giờ phải lớn hơn 0.");
        }
    }
}
