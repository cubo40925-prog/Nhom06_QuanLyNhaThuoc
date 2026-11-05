# Bảng Test Cases – PharmaCare (Đề tài 14: Quản lý nhà thuốc)

**20 test case** (TC01–TC20): mỗi ca một thao tác và một kết quả mong đợi. Tài khoản: `duocsi / duocsi123` (Dược sĩ bán hàng), `quanly / quanly123` (Quản lý kho). Dữ liệu mẫu từ `02_InsertSampleData.sql`.
Cột "Thực tế" và "Đạt" điền sau khi chạy thử trên máy nhóm.

> **Cách chạy:** trên CSDL mới (đóng ứng dụng, chạy `Database/05_ResetDemo.sql`, mở lại ứng dụng), chạy lần lượt từ TC01 đến TC20. `MED011` được tạo ở TC05, sửa ở TC08 và xóa ở TC09. TC16 chỉ kiểm tra giỏ nên **không bấm Thanh toán**.
> Dữ liệu mẫu có hạn dùng tính theo ngày chạy script (`LOT-OR-001` còn 18 ngày), nên hãy reset ngay trước khi test. Bộ ca khó hơn (rollback, lỗi máy in, FEFO nhiều lô...) nằm ở `Docs/KiemThuMoRong.md` (không bắt buộc).

| ID | Chức năng | Dữ liệu / Thao tác | Kết quả mong đợi | Thực tế | Đạt |
|----|-----------|--------------------|------------------|---------|-----|
| TC01 | Đăng nhập – Dược sĩ | `duocsi` / `duocsi123` | Vào màn hình chính, sidebar 7 mục, không có Lô nhập kho |  |  |
| TC02 | Đăng nhập – Quản lý kho | `quanly` / `quanly123` | Vào màn hình chính, sidebar 9 mục, không có Bán hàng |  |  |
| TC03 | Đăng nhập – Sai mật khẩu | `duocsi` / `sai123` | Báo lỗi, không vào được |  |  |
| TC04 | Đăng nhập – Bảo mật | Tên đăng nhập `' OR 1=1 --` | Không đăng nhập được |  |  |
| TC05 | Thêm thuốc – Hợp lệ | Mã mới `MED011`, tên, giá 3000 | Thêm thành công, lưới cập nhật |  |  |
| TC06 | Thêm thuốc – Trùng mã | Mã `MED001` | Báo lỗi trùng mã, không lưu |  |  |
| TC07 | Thêm thuốc – Dữ liệu rỗng | Để trống tên thuốc | Báo lỗi tại ô tên, không lưu |  |  |
| TC08 | Sửa thuốc | Đổi giá `MED011` thành 3500 | Lưu thành công, lưới hiện giá mới |  |  |
| TC09 | Xóa thuốc – Mới tạo | Xóa `MED011` | Xóa thành công, thuốc biến mất khỏi lưới |  |  |
| TC10 | Xóa thuốc – Đã có lô nhập | Xóa `MED001` | Không cho xóa, hiện thông báo |  |  |
| TC11 | Tìm thuốc | Gõ `vitamin` vào ô tìm kiếm | Chỉ còn các thuốc Vitamin |  |  |
| TC12 | Phân quyền – Dược sĩ | `duocsi` mở Danh mục thuốc | Nút Thêm/Sửa/Xóa bị khóa |  |  |
| TC13 | Nhập kho – Lô mới | `quanly`, `MED001`, mã lô `LO-NEW-01`, SL 100, hạn +1 năm | Lô hiện trong bảng |  |  |
| TC14 | Phiếu nhập – Lọc | `quanly`: Phiếu nhập → nhà cung cấp Công ty Dược An Khang → Lọc | Chỉ còn phiếu của An Khang; chọn một phiếu thì thấy chi tiết lô |  |  |
| TC15 | Bán hàng – FEFO | `duocsi`: Oresol SL 20 → Thanh toán | Lấy từ `LOT-OR-001`, tồn giảm 20, có hóa đơn mới |  |  |
| TC16 | Bán hàng – Vượt tồn | Men tiêu hóa (tồn 8): thêm SL 9, không thanh toán | Báo tối đa 8, giỏ không đổi |  |  |
| TC17 | In hóa đơn | Sau TC15: Print Preview hóa đơn vừa tạo | Hóa đơn đúng thuốc và số lượng, tổng cộng bằng tổng thanh toán |  |  |
| TC18 | Hóa đơn – Tra cứu | Trang Hóa đơn: gõ `Nguyễn Văn A` → Lọc | Chỉ còn hóa đơn của A, tổng 28,000 |  |  |
| TC19 | Cảnh báo hạn dùng | Mở trang Cảnh báo | Có dòng đỏ (đã hết hạn), vàng (sắp hết hạn), cam (sắp hết hàng) |  |  |
| TC20 | Báo cáo doanh thu | `quanly` mở Báo cáo doanh thu | Doanh thu theo ngày, tổng khớp tổng các hóa đơn |  |  |
