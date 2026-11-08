# THUYẾT MINH ĐỀ TÀI 14 – QUẢN LÝ NHÀ THUỐC

## 1. Mục tiêu
Xây dựng ứng dụng desktop hỗ trợ quản lý danh mục thuốc, nhóm thuốc, nhà cung cấp, nhập hàng theo lô, theo dõi hạn sử dụng, bán thuốc, lập hóa đơn và thống kê doanh thu.

## 2. Đối tượng sử dụng
Hệ thống có hai vai trò (bảng `Users`, mật khẩu băm PBKDF2-SHA256):
- **Quản lý kho** (`quanly`): thêm/sửa/xóa thuốc, nhập lô, xem lịch sử phiếu nhập, xem hóa đơn, cảnh báo, báo cáo doanh thu.
- **Dược sĩ bán hàng** (`duocsi`): xem danh mục, đổi trạng thái kê đơn, bán hàng (FEFO), in hóa đơn, tra cứu hóa đơn và cảnh báo.
Quyền được kiểm tra ở cả giao diện (ẩn/khóa chức năng) lẫn tầng Service (`AppSession.Require`).

## 3. Công nghệ
C# WinForms trên .NET 10; SQL Server; ADO.NET với Microsoft.Data.SqlClient; truy vấn tham số hóa; Stored Procedure (`sp_Lo_CanhBao`, `sp_Lo_TonTheoLo`, `sp_Lo_Nhap`); kiến trúc MVP kết hợp nhiều lớp (Model – View – Presenter – Service – Repository).

## 4. Các phân hệ
1. Tổng quan dashboard.
2. Danh mục thuốc và nhóm thuốc.
3. Nhà cung cấp.
4. Nhập kho theo lô, lưu ngày nhập và hạn sử dụng; tra cứu lịch sử nhập kho (phiếu nhập) theo khoảng ngày và nhà cung cấp.
5. Bán hàng và lập hóa đơn.
6. Cảnh báo lô sắp hết hạn/hết hạn/tồn thấp.
7. Tra cứu lịch sử bán hàng, in hóa đơn (Print Preview, nhiều trang).
8. Báo cáo doanh thu theo ngày.
9. Đăng nhập, phân quyền theo vai trò.

## 5. Mô hình dữ liệu
- Categories(CategoryID, CategoryName, Description)
- Suppliers(SupplierID, SupplierName, Phone, Email, Address)
- Users(Username, PasswordHash, FullName, Role, IsActive)
- Medicines(MedicineID, MedicineCode, MedicineName, CategoryID, ActiveIngredient, Unit, SellingPrice, IsPrescription, IsActive, ImageFileName, CreatedAt) – `SellingPrice` là giá niêm yết
- ReceiptNotes(ReceiptID, ReceiptDate, SupplierID, CreatedBy, TotalAmount) và ReceiptLines(ReceiptLineID, ReceiptID, BatchID, Quantity, CostPrice) – phiếu nhập và chi tiết
- Batches(BatchID, BatchCode, MedicineID, SupplierID, QuantityReceived, QuantityRemaining, CostPrice, SellingPrice, ReceivedDate, ExpiryDate, CreatedAt) – `SellingPrice` của lô là giá dùng khi bán
- Invoices(InvoiceID, InvoiceDate, CustomerName, TotalAmount, CreatedBy) – `TotalAmount` bằng tổng Quantity × UnitPrice của các dòng
- InvoiceLines(InvoiceLineID, InvoiceID, BatchID, Quantity, UnitPrice)

Quan hệ: một nhóm thuốc có nhiều thuốc; một thuốc có nhiều lô; nhà cung cấp cung cấp nhiều lô; một phiếu nhập có nhiều dòng, mỗi dòng tham chiếu một lô; một hóa đơn có nhiều dòng, mỗi dòng bán tham chiếu đến lô đã xuất; hóa đơn và phiếu nhập ghi người lập (`Users`).

## 6. Quy tắc nghiệp vụ chính
- Không cho số lượng/giá âm.
- Lô hàng lưu riêng ngày nhập, hạn dùng và số lượng còn lại.
- Khi bán hàng, hệ thống kiểm tra lô còn hàng và chưa hết hạn; giao dịch tạo hóa đơn, tạo chi tiết và trừ tồn trong cùng transaction.
- Ưu tiên chọn lô có hạn dùng gần nhất (FEFO – First Expired, First Out), tràn sang lô kế tiếp khi chưa đủ.
- Giá bán khi bán lấy theo từng lô; nhập lại lô đã có thì giữ nguyên giá bán của lô, giá nhập tính bình quân gia quyền.
- Không nhập lô có hạn dùng là hôm nay/quá khứ hoặc trước ngày nhập; nhập lại mã lô với hạn dùng khác bị từ chối (rollback).
- Cảnh báo lô hết hạn, sắp hết hạn trong khoảng ngày cấu hình và lô tồn thấp.
- Không xóa thuốc đã được sử dụng trong dữ liệu lô.
- Lịch sử phiếu nhập chỉ dành cho Quản lý kho (quyền `NhapKho`); Dược sĩ chỉ xem hóa đơn.

## 7. Kiểm thử
Toàn bộ test case (20 ca, TC01–TC20) nằm ở `Docs/TestCases.md` (và `TestCases/TestCase.xlsx`). Không lặp lại bảng ở đây để tránh lệch số liệu.

## 8. Hạn chế và hướng phát triển
Bản đồ án dùng tài khoản demo cục bộ (mật khẩu demo được ghi trong tài liệu); phân hệ nhóm thuốc/nhà cung cấp hiện tập trung tra cứu (chỉ cho đổi trạng thái kê đơn); chưa có xuất Excel, quản lý người dùng, nhật ký thao tác và kiểm thử tự động. Có thể phát triển thêm CRUD nhóm thuốc/nhà cung cấp, báo cáo theo tháng, quản lý tài khoản và bộ unit test cho `BanHangService` (đã cho phép tiêm `Func<DateTime>` để giả lập ngày).

## 9. Thông tin nhóm
- Tên đề tài: Quản lý nhà thuốc (PharmaCare)
- Môn học: 2611COMP101904 – Lập trình trên Windows
- Giảng viên hướng dẫn: Lê Thanh Thoại
- Nhóm: 06
- Thành viên:
  - Nguyễn Đan Trường – 51.01.104.110 – LemAlreadyTaken
  - Phùng Đăng Khoa – 51.01.104.046 – khoaphung1206-hcmue
  - Nguyễn Trọng Đức Hậu – 51.01.104.025 – duchau169
  - Lê Quốc Tuấn – 51.01.104.115 – cubo40925-prog


## Cấu hình kết nối SQL Server trên máy phát triển
Ứng dụng tự dò lần lượt các máy chủ SQL Server thường gặp (`.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`, `.`, tên máy), dùng Windows Authentication và `TrustServerCertificate=True`. Cơ sở dữ liệu có tên `PharmaCareDB`; ứng dụng tự tạo CSDL khi chạy lần đầu (3 script nhúng sẵn), hoặc chạy tay `Database/01_CreateDatabase.sql` → `02_InsertSampleData.sql` → `03_StoredProcedures.sql` trong SSMS.
