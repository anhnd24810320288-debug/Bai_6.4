/* =====================================================
   BÀI 6.4 - GarageDB : Quản lý Garage & Bảo dưỡng xe ô tô
   Chạy toàn bộ script này trong SSMS (SQL Server / LocalDB)
   ===================================================== */
USE master;
GO
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'GarageDB')
BEGIN
    EXEC (N'ALTER DATABASE [GarageDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
    EXEC (N'DROP DATABASE [GarageDB]');
END
GO
CREATE DATABASE [GarageDB];
GO
USE [GarageDB];
GO

CREATE TABLE KhachHang (
    MaKH      INT IDENTITY(1,1) PRIMARY KEY,
    TenKH     NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(15) NULL UNIQUE,
    DiaChi    NVARCHAR(200) NULL
);

CREATE TABLE Xe (
    BienSo      VARCHAR(20) PRIMARY KEY,
    MaKH        INT NOT NULL,
    HieuXe      NVARCHAR(50) NULL,
    NamSanXuat  INT NULL,
    CONSTRAINT FK_Xe_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);

CREATE TABLE PhuTung (
    MaPhuTung  INT IDENTITY(1,1) PRIMARY KEY,
    TenPhuTung NVARCHAR(150) NOT NULL,
    DonGia     DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoLuongTon INT NOT NULL DEFAULT 0,
    CONSTRAINT CK_PhuTung_Ton CHECK (SoLuongTon >= 0)
);

CREATE TABLE PhieuSuaChua (
    MaPhieu    INT IDENTITY(1,1) PRIMARY KEY,
    BienSo     VARCHAR(20) NOT NULL,
    NgayNhan   DATETIME NOT NULL DEFAULT GETDATE(),
    NoiDungSua NVARCHAR(MAX) NULL,
    TongTien   DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai  NVARCHAR(30) NOT NULL DEFAULT N'Đang sửa',
    CONSTRAINT FK_Phieu_Xe FOREIGN KEY (BienSo) REFERENCES Xe(BienSo),
    CONSTRAINT CK_Phieu_TrangThai CHECK (TrangThai IN (N'Đang sửa', N'Đã hoàn thành', N'Đã thanh toán'))
);

CREATE TABLE ChiTietSuaChua (
    MaPhieu    INT NOT NULL,
    MaPhuTung  INT NOT NULL,
    SoLuong    INT NOT NULL,
    TienCong   DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_ChiTietSuaChua PRIMARY KEY (MaPhieu, MaPhuTung),
    CONSTRAINT FK_CT_Phieu   FOREIGN KEY (MaPhieu)   REFERENCES PhieuSuaChua(MaPhieu),
    CONSTRAINT FK_CT_PhuTung FOREIGN KEY (MaPhuTung) REFERENCES PhuTung(MaPhuTung),
    CONSTRAINT CK_CT_SoLuong CHECK (SoLuong > 0)
);
GO

/* ---------------- DỮ LIỆU MẪU ---------------- */
INSERT INTO KhachHang (TenKH, DienThoai, DiaChi) VALUES
(N'Nguyễn Văn Hùng', '0901234567', N'12 Lê Lợi, Hà Nội'),
(N'Trần Thị Mai',    '0912345678', N'45 Nguyễn Trãi, Hà Nội'),
(N'Lê Quốc Bảo',     '0987654321', N'8 Trần Phú, Hải Phòng'),
(N'Phạm Thu Hà',     '0933445566', N'99 Cầu Giấy, Hà Nội');

INSERT INTO Xe (BienSo, MaKH, HieuXe, NamSanXuat) VALUES
('29A-123.45', 1, N'Toyota',  2019),
('30F-678.90', 2, N'Honda',   2021),
('51H-111.22', 3, N'VinFast', 2023),
('29B-555.66', 4, N'Hyundai', 2018),
('29A-999.99', 1, N'Kia',     2020);

INSERT INTO PhuTung (TenPhuTung, DonGia, SoLuongTon) VALUES
(N'Dầu nhớt Castrol 4L',        450000, 50),
(N'Lọc dầu động cơ',            120000, 30),
(N'Lọc gió động cơ',            180000, 25),
(N'Má phanh trước',             850000, 12),
(N'Bugi NGK (cái)',              95000, 40),
(N'Ắc quy GS 12V',             1650000,  6),
(N'Lốp Michelin 205/55R16',    2300000,  2),
(N'Nước làm mát 1L',             70000, 60);

-- Phiếu 1: đã thanh toán (tháng trước)
INSERT INTO PhieuSuaChua (BienSo, NgayNhan, NoiDungSua, TongTien, TrangThai)
VALUES ('29A-123.45', DATEADD(MONTH,-1,GETDATE()), N'Thay dầu nhớt + lọc dầu định kỳ', 720000, N'Đã thanh toán');
INSERT INTO ChiTietSuaChua (MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES (1,1,1,100000),(1,2,1,50000);

-- Phiếu 2: đã hoàn thành, chờ thanh toán
INSERT INTO PhieuSuaChua (BienSo, NgayNhan, NoiDungSua, TongTien, TrangThai)
VALUES ('30F-678.90', DATEADD(DAY,-5,GETDATE()), N'Thay má phanh trước + bugi', 1580000, N'Đã hoàn thành');
INSERT INTO ChiTietSuaChua (MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES (2,4,1,200000),(2,5,4,150000);

-- Phiếu 3: đang sửa
INSERT INTO PhieuSuaChua (BienSo, NgayNhan, NoiDungSua, TongTien, TrangThai)
VALUES ('51H-111.22', GETDATE(), N'Vệ sinh và thay lọc gió', 260000, N'Đang sửa');
INSERT INTO ChiTietSuaChua (MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES (3,3,1,80000);
GO
SELECT 'Tao CSDL GarageDB thanh cong' AS KetQua;
