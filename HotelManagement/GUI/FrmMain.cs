using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HotelManagement.BUS;
using HotelManagement.DTO;

namespace HotelManagement.GUI
{
    /// <summary>
    /// Màn hình chính: danh sách phòng (lọc ngắt kết nối bằng DataView), Check-in, Gọi dịch vụ, Check-out.
    /// </summary>
    public class FrmMain : Form
    {
        private readonly PhongBUS phongBUS = new PhongBUS();
        private readonly KhachHangBUS khBUS = new KhachHangBUS();
        private readonly DichVuBUS dvBUS = new DichVuBUS();
        private readonly DatPhongBUS datPhongBUS = new DatPhongBUS();

        private DataTable dtPhong;     // nạp 1 lần
        private DataView dvPhong;      // lọc trên giao diện

        // Danh sách phòng
        private DataGridView dgvPhong;
        private ComboBox cbLocLoai, cbLocTrangThai;
        private TextBox txtTimPhong;
        private Label lblThongKe;

        // Check-in
        private TabControl tabs;
        private TabPage tabCheckIn, tabDichVu;
        private TextBox txtCCCD, txtHoTen, txtSDT;
        private Label lblKetQuaTraCuu;
        private ComboBox cbPhongTrong;
        private KhachHangDTO khachDaTraCuu;

        // Dịch vụ & Check-out
        private Label lblPhieu, lblHoaDon;
        private ComboBox cbDichVu;
        private NumericUpDown nudSoLuongDV;
        private DataGridView dgvDichVu;
        private PhieuDatPhongDTO phieuDangChon;

        private static readonly Color MAU_TRONG = Color.FromArgb(218, 251, 225);
        private static readonly Color MAU_DANG_O = Color.FromArgb(255, 220, 220);
        private static readonly Color MAU_DANG_DON = Color.FromArgb(255, 243, 205);

        public FrmMain()
        {
            Text = "HOTEL MANAGEMENT - Quản lý khách sạn & dịch vụ";
            Size = new Size(1250, 700);
            MinimumSize = new Size(1100, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);
            TaoGiaoDien();
            Load += (s, e) => UiHelper.ThucHien(TaiDuLieu);
        }

        // ======================= GIAO DIỆN =======================
        private void TaoGiaoDien()
        {
            MenuStrip menu = new MenuStrip();
            menu.Items.Add("Quản lý phòng & dịch vụ", null, (s, e) =>
            {
                using (FrmQuanLy f = new FrmQuanLy()) f.ShowDialog(this);
                UiHelper.ThucHien(TaiDuLieu);
            });
            menu.Items.Add("Báo cáo doanh thu", null, (s, e) => { using (FrmBaoCao f = new FrmBaoCao()) f.ShowDialog(this); });
            menu.Items.Add("Làm mới", null, (s, e) => UiHelper.ThucHien(TaiDuLieu));
            menu.Items.Add("Thoát", null, (s, e) => Close());

            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            root.Controls.Add(TaoKhuDanhSachPhong(), 0, 0);

            tabs = new TabControl { Dock = DockStyle.Fill };
            tabCheckIn = TaoTabCheckIn();
            tabDichVu = TaoTabDichVu();
            tabs.TabPages.Add(tabCheckIn);
            tabs.TabPages.Add(tabDichVu);
            root.Controls.Add(tabs, 1, 0);

            Controls.Add(root);
            Controls.Add(menu);
            MainMenuStrip = menu;
        }

        private Control TaoKhuDanhSachPhong()
        {
            GroupBox gb = new GroupBox { Text = "Danh sách phòng", Dock = DockStyle.Fill };

            FlowLayoutPanel loc = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(2, 6, 2, 0) };
            cbLocLoai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
            cbLocLoai.Items.Add("Tất cả");
            cbLocLoai.Items.AddRange(PhongBUS.LOAI_PHONG);
            cbLocLoai.SelectedIndex = 0;
            cbLocTrangThai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
            cbLocTrangThai.Items.AddRange(new object[] { "Tất cả", PhongDTO.TRONG, PhongDTO.DANG_O, PhongDTO.DANG_DON });
            cbLocTrangThai.SelectedIndex = 0;
            txtTimPhong = new TextBox { Width = 90 };
            loc.Controls.AddRange(new Control[]
            {
                new Label { Text = "Loại:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) }, cbLocLoai,
                new Label { Text = "Trạng thái:", AutoSize = true, Margin = new Padding(8, 5, 0, 0) }, cbLocTrangThai,
                new Label { Text = "Số phòng:", AutoSize = true, Margin = new Padding(8, 5, 0, 0) }, txtTimPhong
            });
            cbLocLoai.SelectedIndexChanged += (s, e) => LocPhong();
            cbLocTrangThai.SelectedIndexChanged += (s, e) => LocPhong();
            txtTimPhong.TextChanged += (s, e) => LocPhong();

            dgvPhong = UiHelper.TaoLuoi();
            dgvPhong.CellFormatting += DgvPhong_CellFormatting;
            dgvPhong.SelectionChanged += (s, e) => UiHelper.ThucHien(ChonPhong);

            FlowLayoutPanel duoi = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(2, 4, 2, 0) };
            lblThongKe = new Label { AutoSize = true, Margin = new Padding(0, 8, 20, 0) };
            duoi.Controls.Add(lblThongKe);
            duoi.Controls.Add(UiHelper.TaoNut("Dọn xong", DonXong, 110));

            gb.Controls.Add(dgvPhong);
            gb.Controls.Add(duoi);
            gb.Controls.Add(loc);
            return gb;
        }

        private TabPage TaoTabCheckIn()
        {
            TabPage tab = new TabPage("Check-in (Nhận phòng)");
            TableLayoutPanel p = UiHelper.TaoKhungNhap(110);

            FlowLayoutPanel dongCCCD = new FlowLayoutPanel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            txtCCCD = new TextBox { Width = 180 };
            dongCCCD.Controls.Add(txtCCCD);
            dongCCCD.Controls.Add(UiHelper.TaoNut("Tra cứu", TraCuuKhach, 90));
            txtCCCD.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) UiHelper.ThucHien(TraCuuKhach); };

            lblKetQuaTraCuu = new Label { AutoSize = true, ForeColor = Color.DimGray, Text = "Nhập CCCD rồi bấm Tra cứu." };
            txtHoTen = new TextBox { Dock = DockStyle.Fill };
            txtSDT = new TextBox { Dock = DockStyle.Fill };
            cbPhongTrong = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

            UiHelper.ThemDong(p, "CCCD:", dongCCCD);
            UiHelper.ThemDong(p, null, lblKetQuaTraCuu);
            UiHelper.ThemDong(p, "Họ tên:", txtHoTen);
            UiHelper.ThemDong(p, "Điện thoại:", txtSDT);
            UiHelper.ThemDong(p, "Phòng trống:", cbPhongTrong);

            Button btnNhan = UiHelper.TaoNut("NHẬN PHÒNG", CheckIn, 160);
            btnNhan.BackColor = Color.FromArgb(9, 105, 218);
            btnNhan.ForeColor = Color.White;
            btnNhan.FlatStyle = FlatStyle.Flat;
            UiHelper.ThemNut(p, btnNhan, UiHelper.TaoNut("Nhập lại", XoaFormCheckIn, 100));

            tab.Controls.Add(p);
            return tab;
        }

        private TabPage TaoTabDichVu()
        {
            TabPage tab = new TabPage("Dịch vụ & Check-out");

            TableLayoutPanel tren = new TableLayoutPanel { Dock = DockStyle.Top, Height = 120, ColumnCount = 3, Padding = new Padding(6) };
            tren.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tren.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            tren.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            lblPhieu = new Label { AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Color.DarkBlue, Text = "Chọn một phòng \"Đang ở\" ở danh sách bên trái." };
            cbDichVu = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            nudSoLuongDV = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 1, Width = 60 };
            tren.Controls.Add(lblPhieu, 0, 0);
            tren.SetColumnSpan(lblPhieu, 3);
            tren.Controls.Add(cbDichVu, 0, 1);
            tren.Controls.Add(nudSoLuongDV, 1, 1);
            tren.Controls.Add(UiHelper.TaoNut("Gọi dịch vụ", GoiDichVu, 115), 2, 1);

            dgvDichVu = UiHelper.TaoLuoi();

            Panel duoi = new Panel { Dock = DockStyle.Bottom, Height = 170, Padding = new Padding(6) };
            lblHoaDon = new Label { Dock = DockStyle.Fill, Font = new Font("Consolas", 10.5F), Text = "" };
            Button btnCheckOut = UiHelper.TaoNut("CHECK-OUT & THANH TOÁN", CheckOut, 240);
            btnCheckOut.Dock = DockStyle.Bottom;
            btnCheckOut.Height = 40;
            btnCheckOut.BackColor = Color.FromArgb(46, 160, 67);
            btnCheckOut.ForeColor = Color.White;
            btnCheckOut.FlatStyle = FlatStyle.Flat;
            duoi.Controls.Add(lblHoaDon);
            duoi.Controls.Add(btnCheckOut);

            tab.Controls.Add(dgvDichVu);
            tab.Controls.Add(duoi);
            tab.Controls.Add(tren);
            return tab;
        }

        // ======================= DỮ LIỆU =======================
        private void TaiDuLieu()
        {
            string phongDangChon = dgvPhong.CurrentRow != null ? Convert.ToString(dgvPhong.CurrentRow.Cells["SoPhong"].Value) : null;

            dtPhong = phongBUS.GetAllTable();
            dvPhong = new DataView(dtPhong);
            dgvPhong.DataSource = dvPhong;
            dgvPhong.Columns["SoPhong"].HeaderText = "Số phòng";
            dgvPhong.Columns["LoaiPhong"].HeaderText = "Loại phòng";
            dgvPhong.Columns["GiaTheoGio"].HeaderText = "Giá / giờ";
            dgvPhong.Columns["GiaTheoGio"].DefaultCellStyle.Format = "N0";
            dgvPhong.Columns["TrangThai"].HeaderText = "Trạng thái";
            LocPhong();

            cbPhongTrong.DataSource = phongBUS.GetPhongTrong();
            cbDichVu.DataSource = dvBUS.GetAll();

            // Giữ lựa chọn phòng cũ sau khi làm mới
            if (phongDangChon != null)
                foreach (DataGridViewRow r in dgvPhong.Rows)
                    if (Convert.ToString(r.Cells["SoPhong"].Value) == phongDangChon) { dgvPhong.CurrentCell = r.Cells[0]; break; }
            ChonPhong();
        }

        /// <summary>Lọc ngắt kết nối: chỉ đổi RowFilter của DataView, không truy vấn lại SQL Server.</summary>
        private void LocPhong()
        {
            if (dvPhong == null) return;
            string loai = cbLocLoai.SelectedIndex > 0 ? cbLocLoai.SelectedItem.ToString() : null;
            string tt = cbLocTrangThai.SelectedIndex > 0 ? cbLocTrangThai.SelectedItem.ToString() : null;
            dvPhong.RowFilter = PhongBUS.TaoBoLoc(loai, tt, txtTimPhong.Text);

            int trong = 0, dangO = 0, dangDon = 0;
            foreach (DataRow r in dtPhong.Rows)
            {
                string s = r["TrangThai"].ToString();
                if (s == PhongDTO.TRONG) trong++; else if (s == PhongDTO.DANG_O) dangO++; else dangDon++;
            }
            lblThongKe.Text = string.Format("Hiển thị {0}/{1} phòng  |  Trống: {2}  Đang ở: {3}  Đang dọn: {4}",
                dvPhong.Count, dtPhong.Rows.Count, trong, dangO, dangDon);
        }

        private void DgvPhong_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string tt = Convert.ToString(dgvPhong.Rows[e.RowIndex].Cells["TrangThai"].Value);
            e.CellStyle.BackColor = tt == PhongDTO.TRONG ? MAU_TRONG : tt == PhongDTO.DANG_O ? MAU_DANG_O : MAU_DANG_DON;
            e.CellStyle.SelectionBackColor = Color.FromArgb(9, 105, 218);
        }

        private void ChonPhong()
        {
            if (dgvPhong.CurrentRow == null) return;
            string soPhong = Convert.ToString(dgvPhong.CurrentRow.Cells["SoPhong"].Value);
            string tt = Convert.ToString(dgvPhong.CurrentRow.Cells["TrangThai"].Value);

            if (tt == PhongDTO.DANG_O)
            {
                phieuDangChon = datPhongBUS.GetPhieuDangO(soPhong);
                tabs.SelectedTab = tabDichVu;
            }
            else
            {
                phieuDangChon = null;
                if (tt == PhongDTO.TRONG)
                {
                    tabs.SelectedTab = tabCheckIn;
                    foreach (object o in cbPhongTrong.Items)
                        if (((PhongDTO)o).SoPhong == soPhong) { cbPhongTrong.SelectedItem = o; break; }
                }
            }
            HienPhieu();
        }

        private void HienPhieu()
        {
            if (phieuDangChon == null)
            {
                lblPhieu.Text = "Chọn một phòng \"Đang ở\" ở danh sách bên trái.";
                dgvDichVu.DataSource = null;
                lblHoaDon.Text = "";
                return;
            }
            lblPhieu.Text = string.Format("Phòng {0} – {1} – nhận lúc {2:dd/MM/yyyy HH:mm}",
                phieuDangChon.SoPhong, phieuDangChon.HoTenKhach, phieuDangChon.NgayNhanPhong);
            dgvDichVu.DataSource = datPhongBUS.GetDichVuDaDung(phieuDangChon.MaPhieu);
            foreach (string c in new[] { "Đơn giá", "Thành tiền" })
                if (dgvDichVu.Columns.Contains(c)) dgvDichVu.Columns[c].DefaultCellStyle.Format = "N0";
            if (dgvDichVu.Columns.Contains("Thời gian")) dgvDichVu.Columns["Thời gian"].DefaultCellStyle.Format = "dd/MM HH:mm";

            HoaDonThanhToanDTO hd = datPhongBUS.TinhTien(phieuDangChon, DateTime.Now);
            lblHoaDon.Text = TaoNoiDungHoaDon(hd);
        }

        private static string TaoNoiDungHoaDon(HoaDonThanhToanDTO hd)
        {
            return string.Format(
                "Tiền phòng : {0,4} giờ x {1,10:N0} = {2,12:N0} đ\n" +
                "Tiền dịch vụ:                      {3,12:N0} đ\n" +
                "-----------------------------------------------\n" +
                "TỔNG CỘNG  :                      {4,12:N0} đ",
                hd.SoGio, hd.GiaTheoGio, hd.TienPhong, hd.TienDichVu, hd.TongTien);
        }

        // ======================= NGHIỆP VỤ =======================
        private void TraCuuKhach()
        {
            khachDaTraCuu = khBUS.TraCuuTheoCCCD(txtCCCD.Text);
            if (khachDaTraCuu != null)
            {
                txtHoTen.Text = khachDaTraCuu.HoTen;
                txtSDT.Text = khachDaTraCuu.SoDienThoai;
                txtHoTen.ReadOnly = txtSDT.ReadOnly = true;
                lblKetQuaTraCuu.ForeColor = Color.DarkGreen;
                lblKetQuaTraCuu.Text = "✔ Khách cũ (Mã KH " + khachDaTraCuu.MaKH + ")";
            }
            else
            {
                txtHoTen.Clear();
                txtSDT.Clear();
                txtHoTen.ReadOnly = txtSDT.ReadOnly = false;
                lblKetQuaTraCuu.ForeColor = Color.DarkOrange;
                lblKetQuaTraCuu.Text = "Khách mới – vui lòng nhập họ tên và số điện thoại.";
                txtHoTen.Focus();
            }
        }

        private void CheckIn()
        {
            string cccd = KhachHangBUS.KiemTraCCCD(txtCCCD.Text);
            KhachHangDTO kh = (khachDaTraCuu != null && khachDaTraCuu.CCCD == cccd)
                ? khachDaTraCuu
                : new KhachHangDTO { CCCD = cccd, HoTen = txtHoTen.Text, SoDienThoai = txtSDT.Text };

            PhongDTO phong = cbPhongTrong.SelectedItem as PhongDTO;
            int maPhieu = datPhongBUS.CheckIn(kh, phong);
            UiHelper.ThongBao("Check-in thành công!\nPhiếu số " + maPhieu + " – Phòng " + phong.SoPhong + " – Khách " + kh.HoTen);
            XoaFormCheckIn();
            TaiDuLieu();
        }

        private void XoaFormCheckIn()
        {
            khachDaTraCuu = null;
            txtCCCD.Clear(); txtHoTen.Clear(); txtSDT.Clear();
            txtHoTen.ReadOnly = txtSDT.ReadOnly = false;
            lblKetQuaTraCuu.ForeColor = Color.DimGray;
            lblKetQuaTraCuu.Text = "Nhập CCCD rồi bấm Tra cứu.";
        }

        private void GoiDichVu()
        {
            datPhongBUS.GoiDichVu(phieuDangChon == null ? 0 : phieuDangChon.MaPhieu, cbDichVu.SelectedItem as DichVuDTO, (int)nudSoLuongDV.Value);
            nudSoLuongDV.Value = 1;
            HienPhieu();
        }

        private void CheckOut()
        {
            if (phieuDangChon == null) throw new ArgumentException("Chọn phòng đang có khách để check-out.");
            DateTime traLuc = DateTime.Now;
            HoaDonThanhToanDTO hd = datPhongBUS.TinhTien(phieuDangChon, traLuc);
            if (!UiHelper.XacNhan("Check-out phòng " + hd.SoPhong + " – khách " + hd.HoTenKhach + "\n\n" + TaoNoiDungHoaDon(hd) + "\n\nXác nhận thanh toán?"))
                return;
            datPhongBUS.CheckOut(hd, traLuc);
            UiHelper.ThongBao("Đã thanh toán " + hd.TongTien.ToString("N0") + " đ.\nPhòng " + hd.SoPhong + " chuyển sang \"Đang dọn dẹp\".");
            TaiDuLieu();
        }

        private void DonXong()
        {
            if (dgvPhong.CurrentRow == null) throw new ArgumentException("Chọn phòng cần đánh dấu dọn xong.");
            phongBUS.DonXong(Convert.ToString(dgvPhong.CurrentRow.Cells["SoPhong"].Value));
            TaiDuLieu();
        }
    }
}
