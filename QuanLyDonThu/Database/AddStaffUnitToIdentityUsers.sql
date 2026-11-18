SET XACT_ABORT ON;

IF DB_NAME() <> N'QuanLyDonThu'
	THROW 51100, 'This script is intended only for the QuanLyDonThu database.', 1;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NULL OR OBJECT_ID(N'dbo.DonVi', N'U') IS NULL
	THROW 51101, 'AspNetUsers or DonVi table is missing; no data was changed.', 1;

IF COL_LENGTH(N'dbo.AspNetUsers', N'MaDonVi') IS NULL
BEGIN
	ALTER TABLE dbo.AspNetUsers ADD MaDonVi INT NULL;

	ALTER TABLE dbo.AspNetUsers
		ADD CONSTRAINT FK_AspNetUsers_DonVi_MaDonVi
		FOREIGN KEY (MaDonVi) REFERENCES dbo.DonVi (MaDonVi);

	CREATE INDEX IX_AspNetUsers_MaDonVi ON dbo.AspNetUsers (MaDonVi);
END;
