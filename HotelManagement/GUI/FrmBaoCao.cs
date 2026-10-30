using System;
using System.Drawing;
using System.Windows.Forms;
using HotelManagement.BUS;

namespace HotelManagement.GUI
{
    /// <summary>Báo cáo doanh thu theo khoảng ngày và theo loại phòng.</summary>
    public class FrmBaoCao : Form
    {
        private readonly DatPhongBUS bus = new DatPhongBUS();
        private readonly DateTimePicker dtpTu, dtpDen;
        private readonly DataGridView dgvLichSu, dgvLoai;
        private readonly Label lblDoanhThu;

        public FrmBaoCao()
        {
            Text = "Báo cáo doanh thu";
            Size = new Size(1100, 600);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);

            FlowLayoutPanel top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            dtpTu = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130, Value = DateTime.Today.AddDays(-7) };
            dtpDen = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 130, Value = DateTime.Today };
            lblDoanhThu = new Label { AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.DarkGreen, Margin = new Padding(30, 6, 0, 0) };
            top.Controls.AddRange(new Control[]
            {
                new Label { Text = "Từ ngày:", AutoSize = true, Margin = new Padding(0, 6, 0, 0) }, dtpTu,
                new Label { Text = "Đến ngày:", AutoSize = true, Margin = new Padding(10, 6, 0, 0) }, dtpDen,
                UiHelper.TaoNut("Xem báo cáo", XemBaoCao, 120), lblDoanhThu
            });

            dgvLichSu = UiHelper.TaoLuoi();
            dgvLoai = UiHelper.TaoLuoi();
            GroupBox gb1 = new GroupBox { Text = "Lịch sử thanh toán", Dock = DockStyle.Fill };
            gb1.Controls.Add(dgvLichSu);
            GroupBox gb2 = new GroupBox { Text = "Doanh thu theo loại phòng", Dock = DockStyle.Fill };
            gb2.Controls.Add(dgvLoai);
            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 720 };
            split.Panel1.Controls.Add(gb1);
            split.Panel2.Controls.Add(gb2);

            Controls.Add(split);
            Controls.Add(top);
            Load += (s, e) => UiHelper.ThucHien(XemBaoCao);
        }

        private void XemBaoCao()
        {
            dgvLichSu.DataSource = bus.GetLichSu(dtpTu.Value, dtpDen.Value);
            dgvLoai.DataSource = bus.GetDoanhThuTheoLoaiPhong(dtpTu.Value, dtpDen.Value);
            lblDoanhThu.Text = "Tổng doanh thu: " + bus.GetDoanhThu(dtpTu.Value, dtpDen.Value).ToString("N0") + " đ";

            foreach (string c in new[] { "Nhận phòng", "Trả phòng" })
                if (dgvLichSu.Columns.Contains(c)) dgvLichSu.Columns[c].DefaultCellStyle.Format = "dd/MM HH:mm";
            if (dgvLichSu.Columns.Contains("Tổng tiền")) dgvLichSu.Columns["Tổng tiền"].DefaultCellStyle.Format = "N0";
            if (dgvLoai.Columns.Contains("Doanh thu")) dgvLoai.Columns["Doanh thu"].DefaultCellStyle.Format = "N0";
        }
    }
}
