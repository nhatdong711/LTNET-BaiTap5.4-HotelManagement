using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using HotelManagement.BUS;
using HotelManagement.DTO;

namespace HotelManagement.GUI
{
    /// <summary>Quản lý danh mục Phòng và Dịch vụ (Thêm / Sửa / Xóa) + xem danh sách khách hàng.</summary>
    public class FrmQuanLy : Form
    {
        private readonly PhongBUS phongBUS = new PhongBUS();
        private readonly DichVuBUS dvBUS = new DichVuBUS();
        private readonly KhachHangBUS khBUS = new KhachHangBUS();

        private DataGridView dgvPhong, dgvDV, dgvKH;
        private TextBox txtSoPhong, txtGiaPhong, txtMaDV, txtTenDV, txtGiaDV;
        private ComboBox cbLoaiPhong;

        public FrmQuanLy()
        {
            Text = "Quản lý phòng, dịch vụ & khách hàng";
            Size = new Size(960, 560);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(TaoTabPhong());
            tabs.TabPages.Add(TaoTabDichVu());
            tabs.TabPages.Add(TaoTabKhachHang());
            Controls.Add(tabs);

            Load += (s, e) => UiHelper.ThucHien(() => { TaiPhong(); TaiDichVu(); dgvKH.DataSource = khBUS.GetAll(); });
        }

        // ---------------- PHÒNG ----------------
        private TabPage TaoTabPhong()
        {
            TabPage tab = new TabPage("Phòng");
            dgvPhong = UiHelper.TaoLuoi();
            dgvPhong.SelectionChanged += (s, e) =>
            {
                if (dgvPhong.CurrentRow == null) return;
                txtSoPhong.Text = Convert.ToString(dgvPhong.CurrentRow.Cells["SoPhong"].Value);
                cbLoaiPhong.SelectedItem = Convert.ToString(dgvPhong.CurrentRow.Cells["LoaiPhong"].Value);
                txtGiaPhong.Text = Convert.ToDecimal(dgvPhong.CurrentRow.Cells["GiaTheoGio"].Value).ToString("0");
            };

            TableLayoutPanel p = UiHelper.TaoKhungNhap();
            txtSoPhong = new TextBox { Dock = DockStyle.Fill };
            cbLoaiPhong = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cbLoaiPhong.Items.AddRange(PhongBUS.LOAI_PHONG);
            txtGiaPhong = new TextBox { Dock = DockStyle.Fill };
            UiHelper.ThemDong(p, "Số phòng:", txtSoPhong);
            UiHelper.ThemDong(p, "Loại phòng:", cbLoaiPhong);
            UiHelper.ThemDong(p, "Giá / giờ:", txtGiaPhong);
            UiHelper.ThemNut(p,
                UiHelper.TaoNut("Thêm", () => { phongBUS.Them(DocPhong()); UiHelper.ThongBao("Đã thêm phòng."); TaiPhong(); }),
                UiHelper.TaoNut("Sửa", () => { phongBUS.Sua(DocPhong()); UiHelper.ThongBao("Đã cập nhật phòng."); TaiPhong(); }),
                UiHelper.TaoNut("Xóa", () =>
                {
                    if (!UiHelper.XacNhan("Xóa phòng " + txtSoPhong.Text + "?")) return;
                    phongBUS.Xoa(txtSoPhong.Text.Trim()); UiHelper.ThongBao("Đã xóa phòng."); TaiPhong();
                }),
                UiHelper.TaoNut("Làm mới", () => { txtSoPhong.Clear(); txtGiaPhong.Clear(); cbLoaiPhong.SelectedIndex = -1; }));

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 540 };
            split.Panel1.Controls.Add(dgvPhong);
            split.Panel2.Controls.Add(p);
            tab.Controls.Add(split);
            return tab;
        }

        private void TaiPhong()
        {
            dgvPhong.DataSource = phongBUS.GetAllTable();
            dgvPhong.Columns["GiaTheoGio"].DefaultCellStyle.Format = "N0";
        }

        private PhongDTO DocPhong()
        {
            return new PhongDTO
            {
                SoPhong = txtSoPhong.Text,
                LoaiPhong = cbLoaiPhong.SelectedItem as string,
                GiaTheoGio = DocSo(txtGiaPhong.Text, "Giá theo giờ")
            };
        }

        // ---------------- DỊCH VỤ ----------------
        private TabPage TaoTabDichVu()
        {
            TabPage tab = new TabPage("Dịch vụ");
            dgvDV = UiHelper.TaoLuoi();
            dgvDV.SelectionChanged += (s, e) =>
            {
                DichVuDTO dv = dgvDV.CurrentRow == null ? null : dgvDV.CurrentRow.DataBoundItem as DichVuDTO;
                if (dv == null) return;
                txtMaDV.Text = dv.MaDV.ToString();
                txtTenDV.Text = dv.TenDV;
                txtGiaDV.Text = dv.DonGia.ToString("0");
            };

            TableLayoutPanel p = UiHelper.TaoKhungNhap();
            txtMaDV = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
            txtTenDV = new TextBox { Dock = DockStyle.Fill };
            txtGiaDV = new TextBox { Dock = DockStyle.Fill };
            UiHelper.ThemDong(p, "Mã DV:", txtMaDV);
            UiHelper.ThemDong(p, "Tên dịch vụ:", txtTenDV);
            UiHelper.ThemDong(p, "Đơn giá:", txtGiaDV);
            UiHelper.ThemNut(p,
                UiHelper.TaoNut("Thêm", () => { dvBUS.Them(DocDV()); UiHelper.ThongBao("Đã thêm dịch vụ."); TaiDichVu(); }),
                UiHelper.TaoNut("Sửa", () => { dvBUS.Sua(DocDV()); UiHelper.ThongBao("Đã cập nhật dịch vụ."); TaiDichVu(); }),
                UiHelper.TaoNut("Xóa", () =>
                {
                    DichVuDTO dv = DocDV();
                    if (!UiHelper.XacNhan("Xóa dịch vụ \"" + dv.TenDV + "\"?")) return;
                    dvBUS.Xoa(dv.MaDV); UiHelper.ThongBao("Đã xóa dịch vụ."); TaiDichVu();
                }),
                UiHelper.TaoNut("Làm mới", () => { txtMaDV.Clear(); txtTenDV.Clear(); txtGiaDV.Clear(); }));

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 540 };
            split.Panel1.Controls.Add(dgvDV);
            split.Panel2.Controls.Add(p);
            tab.Controls.Add(split);
            return tab;
        }

        private void TaiDichVu()
        {
            dgvDV.DataSource = dvBUS.GetAll();
            dgvDV.Columns["DonGia"].DefaultCellStyle.Format = "N0";
        }

        private DichVuDTO DocDV()
        {
            int ma;
            int.TryParse(txtMaDV.Text, out ma);
            return new DichVuDTO { MaDV = ma, TenDV = txtTenDV.Text, DonGia = DocSo(txtGiaDV.Text, "Đơn giá") };
        }

        // ---------------- KHÁCH HÀNG ----------------
        private TabPage TaoTabKhachHang()
        {
            TabPage tab = new TabPage("Khách hàng");
            dgvKH = UiHelper.TaoLuoi();
            tab.Controls.Add(dgvKH);
            return tab;
        }

        private static decimal DocSo(string s, string tenTruong)
        {
            decimal v;
            if (!decimal.TryParse((s ?? "").Trim().Replace(",", "").Replace(".", ""), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out v))
                throw new ArgumentException(tenTruong + " phải là số (ví dụ: 120000).");
            return v;
        }
    }
}
