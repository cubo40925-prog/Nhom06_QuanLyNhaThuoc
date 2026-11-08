/* 04_BackupDatabase.sql - tạo và khôi phục file PharmaCareDB.bak.
   Cách tạo: đóng ứng dụng, chạy 05_ResetDemo.sql, mở ứng dụng 1 lần (để tạo CSDL + 2 tài khoản demo),
   đóng ứng dụng rồi chạy PHẦN 1. Không chạy test trước khi sao lưu. */

-- ===== PHẦN 1: SAO LƯU =====
USE master;
GO
DECLARE @dir nvarchar(260) = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(260));
IF RIGHT(@dir, 1) <> N'\' SET @dir += N'\';
DECLARE @file nvarchar(300) = @dir + N'PharmaCareDB.bak';
BACKUP DATABASE PharmaCareDB TO DISK = @file WITH INIT, FORMAT, NAME = N'PharmaCareDB-Full', STATS = 10;
RESTORE VERIFYONLY FROM DISK = @file;
SELECT @file AS FileDaLuuTai;
GO

-- ===== PHẦN 2: KHÔI PHỤC (chạy trên máy khác; đổi đường dẫn @bak) =====
/*
USE master;
GO
DECLARE @bak nvarchar(300) = N'C:\Temp\PharmaCareDB.bak';   -- đổi đường dẫn tới file .bak
DECLARE @mdf nvarchar(300) = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(260));
DECLARE @ldf nvarchar(300) = CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(260));
IF RIGHT(@mdf, 1) <> N'\' SET @mdf += N'\';
IF RIGHT(@ldf, 1) <> N'\' SET @ldf += N'\';
SET @mdf += N'PharmaCareDB.mdf';
SET @ldf += N'PharmaCareDB_log.ldf';
RESTORE DATABASE PharmaCareDB FROM DISK = @bak
    WITH REPLACE, MOVE N'PharmaCareDB' TO @mdf, MOVE N'PharmaCareDB_log' TO @ldf;
GO
*/
