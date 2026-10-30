/* =========================================================
   BÀI TẬP 5.4 - HỆ THỐNG QUẢN LÝ TIỆN ÍCH DỊCH VỤ & KHÁCH SẠN
   Script tạo CSDL HotelDB + dữ liệu mẫu
   ========================================================= */
USE master;
GO
IF DB_ID(N'HotelDB') IS NOT NULL
BEGIN
    ALTER DATABASE HotelDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HotelDB;
END
GO
CREATE DATABASE HotelDB;
GO
USE HotelDB;
GO

-- 1. Phòng
CREATE TABLE Phong (
    SoPhong    VARCHAR(10) PRIMARY KEY,
    LoaiPhong  NVARCHAR(50) NOT NULL,
    GiaTheoGio DECIMAL(18,2) NOT NULL CHECK (GiaTheoGio >= 0),
    TrangThai  NVARCHAR(30) NOT NULL DEFAULT N'Trống'
        CONSTRAINT CK_Phong_TrangThai CHECK (TrangThai IN (N'Trống', N'Đang ở', N'Đang dọn dẹp'))
);

-- 2. Khách hàng
CREATE TABLE KhachHang (
    MaKH        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    CCCD        VARCHAR(20) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15)
);

-- 3. Dịch vụ
CREATE TABLE DichVu (
    MaDV   INT IDENTITY(1,1) PRIMARY KEY,
    TenDV  NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0)
);

-- 4. Phiếu đặt phòng
CREATE TABLE PhieuDatPhong (
    MaPhieu           INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong           VARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Phong(SoPhong),
    MaKH              INT NOT NULL FOREIGN KEY REFERENCES KhachHang(MaKH),
    NgayNhanPhong     DATETIME NOT NULL DEFAULT GETDATE(),
    NgayTraPhong      DATETIME NULL,
    TongTienThanhToan DECIMAL(18,2) NULL
);

-- 5. Sử dụng dịch vụ
CREATE TABLE SuDungDichVu (
    MaSuDung INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieu  INT NOT NULL FOREIGN KEY REFERENCES PhieuDatPhong(MaPhieu),
    MaDV     INT NOT NULL FOREIGN KEY REFERENCES DichVu(MaDV),
    SoLuong  INT NOT NULL CHECK (SoLuong > 0),
    ThoiGian DATETIME NOT NULL DEFAULT GETDATE()
);
GO

/* ---------------- DỮ LIỆU MẪU ---------------- */
INSERT INTO Phong (SoPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES
('P101', N'Đơn', 80000,  N'Trống'),
('P102', N'Đơn', 80000,  N'Trống'),
('P103', N'Đôi', 120000, N'Trống'),
('P104', N'Đôi', 120000, N'Đang dọn dẹp'),
('P201', N'Đơn', 90000,  N'Trống'),
('P202', N'Đôi', 130000, N'Đang ở'),
('P203', N'VIP', 250000, N'Trống'),
('P301', N'VIP', 300000, N'Trống'),
('P302', N'VIP', 300000, N'Đang ở');

INSERT INTO KhachHang (HoTen, CCCD, SoDienThoai) VALUES
(N'Nguyễn Văn An',  '001203004567', '0912345678'),
(N'Trần Thị Bình',  '001304005678', '0987654321'),
(N'Lê Hoàng Cường', '036205001234', '0901112233');

INSERT INTO DichVu (TenDV, DonGia) VALUES
(N'Nước suối', 15000), (N'Nước ngọt', 20000), (N'Bia', 30000),
(N'Giặt là (1 kg)', 40000), (N'Bữa sáng', 80000), (N'Mì ly', 25000);

-- 2 phòng đang có khách
INSERT INTO PhieuDatPhong (SoPhong, MaKH, NgayNhanPhong) VALUES
('P202', 1, DATEADD(HOUR, -5, GETDATE())),
('P302', 2, DATEADD(HOUR, -26, GETDATE()));
INSERT INTO SuDungDichVu (MaPhieu, MaDV, SoLuong, ThoiGian) VALUES
(1, 1, 2, DATEADD(HOUR, -4, GETDATE())),
(2, 5, 2, DATEADD(HOUR, -12, GETDATE())),
(2, 4, 1, DATEADD(HOUR, -10, GETDATE()));

-- Lịch sử đã thanh toán (phục vụ báo cáo)
INSERT INTO PhieuDatPhong (SoPhong, MaKH, NgayNhanPhong, NgayTraPhong, TongTienThanhToan) VALUES
('P101', 3, DATEADD(DAY, -2, GETDATE()), DATEADD(HOUR, -36, GETDATE()), 990000),
('P203', 1, DATEADD(DAY, -1, GETDATE()), DATEADD(HOUR, -20, GETDATE()), 1000000);
INSERT INTO SuDungDichVu (MaPhieu, MaDV, SoLuong, ThoiGian) VALUES
(3, 3, 1, DATEADD(HOUR, -40, GETDATE()));
GO

SELECT * FROM Phong;
