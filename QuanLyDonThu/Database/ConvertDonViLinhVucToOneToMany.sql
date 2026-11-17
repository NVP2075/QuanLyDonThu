SET XACT_ABORT ON;

IF DB_NAME() <> N'QuanLyDonThu'
	THROW 51000, 'This script is intended only for the QuanLyDonThu database.', 1;

IF OBJECT_ID(N'dbo.DonVi', N'U') IS NULL
   OR OBJECT_ID(N'dbo.LinhVuc', N'U') IS NULL
   OR OBJECT_ID(N'dbo.DonVi_LinhVuc', N'U') IS NULL
   OR OBJECT_ID(N'dbo.DonThu', N'U') IS NULL
   OR OBJECT_ID(N'dbo.DotTiepNhan', N'U') IS NULL
   OR OBJECT_ID(N'dbo.LichSuXuLy', N'U') IS NULL
   OR OBJECT_ID(N'dbo.PhanHoi', N'U') IS NULL
   OR OBJECT_ID(N'dbo.TepDinhKem', N'U') IS NULL
	THROW 51001, 'One or more expected tables are missing; no data was changed.', 1;

IF COL_LENGTH(N'dbo.LinhVuc', N'MaDonVi') IS NOT NULL
	THROW 51002, 'LinhVuc.MaDonVi already exists; no data was changed.', 1;

BEGIN TRY
	BEGIN TRANSACTION;

	DELETE FROM dbo.TepDinhKem;
	DELETE FROM dbo.PhanHoi;
	DELETE FROM dbo.LichSuXuLy;
	DELETE FROM dbo.DonThu;
	DELETE FROM dbo.DotTiepNhan;
	DELETE FROM dbo.DonVi_LinhVuc;

	UPDATE dbo.DonVi
	SET MaDonViCha = NULL
	WHERE MaDonViCha IS NOT NULL;

	DELETE FROM dbo.LinhVuc;
	DELETE FROM dbo.DonVi;

	DROP TABLE dbo.DonVi_LinhVuc;

	ALTER TABLE dbo.LinhVuc
		ADD MaDonVi INT NOT NULL;

	CREATE INDEX IX_LinhVuc_MaDonVi
		ON dbo.LinhVuc (MaDonVi);

	ALTER TABLE dbo.LinhVuc
		ADD CONSTRAINT FK_LinhVuc_DonVi_MaDonVi
		FOREIGN KEY (MaDonVi) REFERENCES dbo.DonVi (MaDonVi);

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0
		ROLLBACK TRANSACTION;

	THROW;
END CATCH;
