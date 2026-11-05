# BIÊN BẢN KIỂM THỬ – PHARMACARE

Bảng **20 test case** (TC01–TC20) nằm ở `Docs/TestCases.md` – đó là nguồn duy nhất của các ca kiểm thử. Bảng dưới là bản tóm tắt theo chức năng, mỗi dòng trỏ về ID trong `TestCases.md`. Điền kết quả sau khi chạy ứng dụng trên máy của nhóm.

> Reset dữ liệu (`Database/05_ResetDemo.sql`, mở lại ứng dụng) ngay trước khi test, vì dữ liệu mẫu có hạn dùng tính theo ngày chạy script.

| STT | Chức năng | Test case | Dữ liệu thử | Kết quả mong đợi | Kết quả thực tế | Đạt/Không đạt |
|---|---|---|---|---|---|---|
| 1 | Đăng nhập, phân quyền | TC01–TC04, TC12 | `duocsi` / `quanly`, sai mật khẩu, `' OR 1=1 --` | Đúng vai trò; sai thì báo lỗi | | |
| 2 | Thêm thuốc, dữ liệu sai | TC05–TC07 | Mã mới; mã `MED001`; tên trống | Thêm được; hai ca sai bị từ chối | | |
| 3 | Sửa, xóa, khóa ngoại | TC08–TC10 | Sửa giá `MED011`; xóa `MED011`; xóa `MED001` | Sửa, xóa được; thuốc đã có lô không cho xóa | | |
| 4 | Tìm thuốc | TC11 | `vitamin` | Chỉ còn thuốc Vitamin | | |
| 5 | Nhập kho, phiếu nhập | TC13, TC14 | Lô `LO-NEW-01`; lọc An Khang | Lô hiện trong bảng; lọc đúng nhà cung cấp | | |
| 6 | Bán hàng | TC15, TC16 | Oresol SL 20; Men tiêu hóa SL 9 | Có hóa đơn, trừ tồn; vượt tồn bị từ chối | | |
| 7 | In và tra cứu hóa đơn | TC17, TC18 | Print Preview; `Nguyễn Văn A` | Hóa đơn đúng; tra cứu đúng | | |
| 8 | Cảnh báo, báo cáo | TC19, TC20 | Trang Cảnh báo; Báo cáo doanh thu | Màu đỏ/vàng/cam; doanh thu khớp hóa đơn | | |

Ảnh chụp màn hình cần bổ sung: đăng nhập, dashboard, danh mục thuốc, nhập lô, bán hàng, cảnh báo hạn dùng và báo cáo doanh thu.
