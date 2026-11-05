/* 05_ResetDemo.sql - đưa dữ liệu về trạng thái mẫu ban đầu trước khi demo / trước khi chạy test.
   Chạy trong SSMS (không nhúng vào ứng dụng). ĐÓNG ứng dụng PharmaCare trước.
   Sau khi chạy: mở lại ứng dụng, nó tự tạo lại CSDL + dữ liệu mẫu + 2 tài khoản (xem DbSetup.cs).
   Lưu ý: xóa toàn bộ dữ liệu hiện có. Ảnh thuốc đã thêm trong thư mục Images/ không bị xóa. */
USE master;
GO
IF DB_ID(N'PharmaCareDB') IS NOT NULL
BEGIN
    ALTER DATABASE PharmaCareDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PharmaCareDB;
END
GO
