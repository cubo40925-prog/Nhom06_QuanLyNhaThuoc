# Bảng Test Cases – PharmaCare (Đề tài 14: Quản lý nhà thuốc)

**20 test case** (TC01–TC20): mỗi ca một thao tác và một kết quả mong đợi. Tài khoản: `duocsi / duocsi123` (Dược sĩ bán hàng), `quanly / quanly123` (Quản lý kho). Dữ liệu mẫu từ `02_InsertSampleData.sql`.
Cột "Thực tế" và "Đạt" đã được điền sau khi chạy thử trên máy nhóm.

> **Cách chạy:** trên CSDL mới (đóng ứng dụng, chạy `Database/05_ResetDemo.sql`, mở lại ứng dụng), chạy lần lượt từ TC01 đến TC20. `MED011` được tạo ở TC05, sửa ở TC08 và xóa ở TC09. TC16 chỉ kiểm tra giỏ nên **không bấm Thanh toán**.
> Dữ liệu mẫu có hạn dùng tính theo ngày chạy script (`LOT-OR-001` còn 18 ngày), nên hãy reset ngay trước khi test. Bộ ca khó hơn (rollback, lỗi máy in, FEFO nhiều lô...) nằm ở `Docs/KiemThuMoRong.md` (không bắt buộc).

| ID | Chức năng | Dữ liệu / Thao tác | Kết quả mong đợi | Thực tế | Đạt |
|----|-----------|--------------------|------------------|---------|-----|
| TC01 | Đăng nhập – Dược sĩ | `duocsi` / `duocsi123` | Vào màn hình chính, sidebar 7 mục, không có Lô nhập kho | Vào được màn hình chính, sidebar hiển thị 7 mục, không có Lô nhập kho | ✅ Đạt |
| TC02 | Đăng nhập – Quản lý kho | `quanly` / `quanly123` | Vào màn hình chính, sidebar 9 mục, không có Bán hàng | Vào được màn hình chính, sidebar hiển thị 9 mục, không có Bán hàng | ✅ Đạt |
| TC03 | Đăng nhập – Sai mật khẩu | `duocsi` / `sai123` | Báo lỗi, không vào được | Hiện thông báo lỗi sai mật khẩu, không vào được hệ thống | ✅ Đạt |
| TC04 | Đăng nhập – Bảo mật | Tên đăng nhập `' OR 1=1 --` | Không đăng nhập được | Không đăng nhập được, chuỗi SQL injection bị xử lý như tên đăng nhập thường | ✅ Đạt |
| TC05 | Thêm thuốc – Hợp lệ | Mã mới `MED011`, tên, giá 3000 | Thêm thành công, lưới cập nhật | Thêm thành công, lưới hiện thêm thuốc `MED011` | ✅ Đạt |
| TC06 | Thêm thuốc – Trùng mã | Mã `MED001` | Báo lỗi trùng mã, không lưu | Báo lỗi trùng mã, dữ liệu không được lưu | ✅ Đạt |
| TC07 | Thêm thuốc – Dữ liệu rỗng | Để trống tên thuốc | Báo lỗi tại ô tên, không lưu | Báo lỗi tại ô tên thuốc, dữ liệu không được lưu | ✅ Đạt |
| TC08 | Sửa thuốc | Đổi giá `MED011` thành 3500 | Lưu thành công, lưới hiện giá mới | Lưu thành công, lưới hiện giá mới 3,500 | ✅ Đạt |
| TC09 | Xóa thuốc – Mới tạo | Xóa `MED011` | Xóa thành công, thuốc biến mất khỏi lưới | Xóa thành công, `MED011` biến mất khỏi lưới | ✅ Đạt |
| TC10 | Xóa thuốc – Đã có lô nhập | Xóa `MED001` | Không cho xóa, hiện thông báo | Không cho xóa, hiện thông báo thuốc đã có lô nhập | ✅ Đạt |
| TC11 | Tìm thuốc | Gõ `vitamin` vào ô tìm kiếm | Chỉ còn các thuốc Vitamin | Lưới chỉ còn các thuốc Vitamin | ✅ Đạt |
| TC12 | Phân quyền – Dược sĩ | `duocsi` mở Danh mục thuốc | Nút Thêm/Sửa/Xóa bị khóa | Các nút Thêm/Sửa/Xóa bị vô hiệu hóa | ✅ Đạt |
| TC13 | Nhập kho – Lô mới | `quanly`, `MED001`, mã lô `LO-NEW-01`, SL 100, hạn +1 năm | Lô hiện trong bảng | Nhập thành công, lô `LO-NEW-01` hiện trong bảng với SL 100 | ✅ Đạt |
| TC14 | Phiếu nhập – Lọc | `quanly`: Phiếu nhập → nhà cung cấp Công ty Dược An Khang → Lọc | Chỉ còn phiếu của An Khang; chọn một phiếu thì thấy chi tiết lô | Chỉ còn các phiếu của An Khang; chọn một phiếu hiển thị đúng chi tiết lô | ✅ Đạt |
| TC15 | Bán hàng – FEFO | `duocsi`: Oresol SL 20 → Thanh toán | Lấy từ `LOT-OR-001`, tồn giảm 20, có hóa đơn mới | Thanh toán thành công, trừ 20 từ `LOT-OR-001`, tồn giảm 20, phát sinh hóa đơn mới | ✅ Đạt |
| TC16 | Bán hàng – Vượt tồn | Men tiêu hóa (tồn 8): thêm SL 9, không thanh toán | Báo tối đa 8, giỏ không đổi | Báo số lượng tối đa là 8, giỏ hàng không thay đổi | ✅ Đạt |
| TC17 | In hóa đơn | Sau TC15: Print Preview hóa đơn vừa tạo | Hóa đơn đúng thuốc và số lượng, tổng cộng bằng tổng thanh toán | Bản xem trước đúng thuốc và số lượng, tổng cộng khớp tổng thanh toán | ✅ Đạt |
| TC18 | Hóa đơn – Tra cứu | Trang Hóa đơn: gõ `Nguyễn Văn A` → Lọc | Chỉ còn hóa đơn của A, tổng 28,000 | Chỉ còn hóa đơn của Nguyễn Văn A, tổng 28,000 | ✅ Đạt |
| TC19 | Cảnh báo hạn dùng | Mở trang Cảnh báo | Có dòng đỏ (đã hết hạn), vàng (sắp hết hạn), cam (sắp hết hàng) | Hiển thị đủ dòng đỏ (đã hết hạn), vàng (sắp hết hạn), cam (sắp hết hàng) | ✅ Đạt |
| TC20 | Báo cáo doanh thu | `quanly` mở Báo cáo doanh thu | Doanh thu theo ngày, tổng khớp tổng các hóa đơn | Hiển thị doanh thu theo ngày, tổng khớp tổng các hóa đơn | ✅ Đạt |

**Tổng kết:** 20/20 test case đạt.
