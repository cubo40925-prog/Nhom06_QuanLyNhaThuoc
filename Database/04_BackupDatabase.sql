/* 04_BackupDatabase.sql - tạo file .bak để nộp kèm (đổi đường dẫn cho phù hợp máy của bạn). Chạy SAU 01 -> 03. */
BACKUP DATABASE PharmaCareDB TO DISK = N'C:\Temp\PharmaCareDB.bak' WITH INIT, NAME = N'PharmaCareDB-Full', STATS = 10;
-- Restore: RESTORE DATABASE PharmaCareDB FROM DISK = N'C:\Temp\PharmaCareDB.bak' WITH REPLACE;
