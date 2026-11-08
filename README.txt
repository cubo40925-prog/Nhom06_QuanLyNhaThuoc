PHARMACARE - QUẢN LÝ NHÀ THUỐC (Đề tài 14, 2611COMP101904 – Lập trình trên Windows)
Nhóm: 06   Giảng viên hướng dẫn: Lê Thanh Thoại
Thành viên: Nguyễn Đan Trường (51.01.104.110), Phùng Đăng Khoa (51.01.104.046),
            Nguyễn Trọng Đức Hậu (51.01.104.025), Lê Quốc Tuấn (51.01.104.115)
GitHub: https://github.com/cubo40925-prog/Nhom06_QuanLyNhaThuoc

1. MÔI TRƯỜNG
- Windows 10/11, Visual Studio 2026 (workload ".NET desktop development"), .NET 10 SDK.
- SQL Server Express / Developer (hoặc LocalDB) + SSMS.
- Thư viện: Microsoft.Data.SqlClient (NuGet, tự khôi phục khi build).

2. CẤU TRÚC THƯ MỤC NỘP
SourceCode/   mã nguồn C# (mở PharmaCare.sln) và thư mục Docs (tài liệu bổ sung)
Database/     01_CreateDatabase.sql, 02_InsertSampleData.sql, 03_StoredProcedures.sql,
              04_BackupDatabase.sql, 05_ResetDemo.sql, PharmaCareDB.bak (bản sao lưu CSDL với dữ liệu mẫu)
Report/       PharmaCare_Report.pdf (báo cáo), PharmaCare_Report_latex.zip (mã LaTeX),
              PharmaCare_Slides.pptx (slide thuyết trình)
Screenshots/  TestCases/TC01.png ... TC20.png (minh chứng test)
TestCases/    TestCase.xlsx
README.txt    file này

3. CÁCH CHẠY (nhanh nhất)
1) Mở SourceCode/PharmaCare.sln bằng Visual Studio.
2) Chọn cấu hình Release, nhấn F5.
3) Ứng dụng tự dò SQL Server (.\SQLEXPRESS, (localdb)\MSSQLLocalDB, ., tên máy) và tự tạo CSDL
   PharmaCareDB + dữ liệu mẫu từ 3 script nhúng sẵn. Nếu lỗi, bấm "Chi tiết lỗi" ở màn hình đăng nhập.
4) Chuỗi kết nối nằm ở SourceCode/PharmaCare/AppConfig.cs (Windows Authentication,
   TrustServerCertificate=True). Nếu SQL Server tên khác, sửa AppConfig.Server.

4. TÀI KHOẢN DEMO
- Dược sĩ bán hàng: duocsi / duocsi123  (bán hàng, hóa đơn, cảnh báo)
- Quản lý kho:      quanly / quanly123  (danh mục, nhập kho, phiếu nhập, báo cáo doanh thu)

5. CƠ SỞ DỮ LIỆU (chạy thủ công trong SSMS nếu cần, theo thứ tự)
1) 01_CreateDatabase.sql  2) 02_InsertSampleData.sql  3) 03_StoredProcedures.sql
- Khôi phục từ file .bak: chạy PHẦN 2 trong Database/04_BackupDatabase.sql (đổi đường dẫn tới file .bak).
- Đặt lại dữ liệu mẫu trước khi demo/test: đóng ứng dụng, chạy 05_ResetDemo.sql, mở lại ứng dụng.

6. KIỂM THỬ
- 20 test case (TC01-TC20) trong TestCases/TestCase.xlsx (cũng có trong Docs/TestCases.md).
- Reset dữ liệu (05_ResetDemo.sql, mở lại ứng dụng) rồi chạy lần lượt TC01 đến TC20.

7. KIẾN TRÚC
MVP kết hợp nhiều lớp (Model - View - Presenter - Service - Repository).
Views = GUI, Services = BLL, Repositories = DAL, Models = DTO. 100% truy vấn dùng SqlParameter.
