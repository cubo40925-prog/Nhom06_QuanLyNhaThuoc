# Các thay đổi trong bản vá

- Test: rút gọn bộ test chính thức còn **20 ca** (TC01–TC20), mỗi ca một thao tác và một kết quả mong đợi theo mẫu của giảng viên. Bộ 28 ca cũ chuyển thành `Docs/KiemThuMoRong.md` (MR01–MR28, không bắt buộc). Các chú thích TC trong `NhapKhoPresenter` đã bỏ.

- Test: thêm **TC27** (sửa thuốc) và **TC28** (xóa thuốc và khóa ngoại), tổng **28 ca**; TC01/TC02 thêm bước bấm từng mục sidebar để kiểm tra giao diện không vỡ. TC27/TC28 chạy sau TC05.

- Thêm `Database/05_ResetDemo.sql` (chỉ xóa `PharmaCareDB` để ứng dụng tự tạo lại dữ liệu mẫu; không nhúng vào ứng dụng) và ghi chú trong README.

## Tính năng mới: lịch sử phiếu nhập (đề bài: tra cứu lịch sử NHẬP và BÁN)
- Trang **Phiếu nhập** (sidebar, chỉ `quanly`, dùng quyền `NhapKho`): danh sách phiếu ở trên (lọc theo từ khóa mã phiếu/người lập, nhà cung cấp, khoảng ngày), chi tiết các lô của phiếu đang chọn ở dưới; thanh trạng thái cho số phiếu và tổng giá trị nhập. Cùng mẫu với trang Hóa đơn.
- Mã mới: `Views/ReceiptsPage.cs`, `Presenters/PhieuNhapPresenter.cs`, `IPhieuNhapView` (trong `IViews.cs`); `IKhoRepository`/`KhoRepository.GetPhieuNhap` + `GetChiTietPhieuNhap`; `NhapKhoService.GetPhieuNhap` (kiểm tra quyền + khoảng ngày) và `GetChiTietPhieuNhap`; `MainForm` thêm mục “Phiếu nhập”.
- `02_InsertSampleData.sql`: thêm 11 phiếu nhập mẫu (mỗi lô một phiếu, `PN000005` có 2 dòng); lô `LOT-IB-001` đổi ngày nhập từ -20 sang -30 ngày để cùng ngày với `LOT-PA-001`. Tổng mẫu: 11 phiếu, 5,598,000; Công ty Dược An Khang: 5 phiếu, 1,418,000.
- Test: thêm **TC26** (tổng **26 ca**), TC02 cập nhật sidebar Quản lý kho thành 9 mục; TC26 (a)–(e) phải chạy trước TC09/TC11/TC14.
- Tài liệu: README, THUYET_MINH, BAO_CAO_KIEM_THU cập nhật. Báo cáo LaTeX: chương 3 (ma trận quyền, FR12, BR9), 4 (dữ liệu mẫu), 5, 6, 7 (mục mới + khung ảnh `receipts.png`), 8, 9, use case UC12; `tc-table.tex` tạo lại (75 trang, cùng số cảnh báo hộp như trước).

## Tài liệu / cấu hình
- `PharmaCare.csproj`: giữ `net10.0-windows7.0`; README và THUYET_MINH được sửa từ “.NET 8” thành “.NET 10” (Visual Studio 2026 + .NET 10 SDK) cho khớp.
- Bỏ `bin/`, `obj/`, `*.csproj.user` khỏi gói nộp (16 MB → dưới 1 MB).
- `THUYET_MINH_DE_TAI.md` cập nhật theo đúng ứng dụng (2 vai trò, có in hóa đơn/ảnh thuốc/phân quyền, không còn nhắc SQL View, đủ bảng `Users`, `ReceiptNotes`, `ReceiptLines`).
- Gộp 3 bảng test thành 1 nguồn (`TestCases.md`, **25 ca**, TC01–TC25); `BAO_CAO_KIEM_THU.md` trỏ về ID; README chỉ trỏ tới `TestCases.md`, không chép lại bảng.

## Test case
- Rút gọn từ 53 xuống 25 ca bằng cách gộp các ca liên quan thành một ca có nhiều bước nhỏ (a)/(b)/(c).
- Tổng hóa đơn của Nguyễn Văn A: `32,000` → `28,000` (TC21); sửa dữ liệu mẫu `02_InsertSampleData.sql` cho khớp chi tiết.
- Bỏ ca “ô số lượng không hợp lệ”: ô số lượng chỉ nhận chữ số và tự về tối thiểu 1 nên giao diện không thể gửi số lượng sai. Việc này được nêu ở phần cài đặt thay vì làm test case.
- TC11/TC12 nêu rõ phải đọc hạn dùng hiện có ở bảng bên phải; TC11 cập nhật theo quy tắc giá mới (giữ giá bán của lô); TC17 (rollback) và TC19 (in hóa đơn) cập nhật.
- Ghi chú đầu `TestCases.md`: chạy lại script trước khi test vì hạn dùng mẫu tính theo ngày chạy (ảnh hưởng TC13, 16, 22, 23).
- Các lỗi vừa sửa được kiểm chứng trong 25 ca:

| Lỗi đã sửa | Ca kiểm chứng |
|---|---|
| Giá bán điền sẵn khi nhập lô | TC09 |
| Hạn dùng trước ngày nhập hiện sai ô | TC10 |
| Giỏ hàng sau khi thanh toán thất bại | TC17 |
| Hóa đơn nhiều trang | TC19 |
| Lỗi máy in sau khi đã lưu hóa đơn | TC20 |
| Ký tự đặc biệt và khoảng ngày sai khi tìm hóa đơn | TC21 |
| Thiếu stored procedure không làm mất dữ liệu | TC04(b) |

- Sửa chú thích trong `NhapKhoPresenter.cs` cho khớp đánh số mới: `TC17` → `TC10(a)`, `TC18` → `TC12`, `TC47` → `TC10(b)`.
- Thống nhất cách gọi kiến trúc thành “MVP kết hợp nhiều lớp” (THUYET_MINH mục 3, README); cập nhật đoạn cấu hình kết nối (tự dò nhiều máy chủ); `BAO_CAO_KIEM_THU.md` thêm TC07 vào dòng phân quyền.

## Mã nguồn
- `ThuocService.Luu` / `ImageHelper`: đọc thử ảnh trước, ghi file `Images/<MaThuoc>.png` chỉ SAU khi lưu CSDL thành công (thêm mã trùng không còn ghi đè ảnh của thuốc đang có; TC06a, TC05).
- `HoaDonPresenter` / `IHoaDonView.ShowWarning`: lỗi “Từ ngày sau Đến ngày” hiện cảnh báo, giữ nguyên bảng, không báo “chưa kết nối CSDL” (TC21).
- `ReceivePage.ShowThuoc/ShowNhaCungCap`: tải lại danh sách không làm mất thuốc, nhà cung cấp và giá bán đang nhập.
- `NhapKhoPresenter`: ngày nhập sai báo lỗi tại ô Ngày nhập (`NhapField.NgayNhap`) thay vì tự đặt thành hôm nay; giá nhập/giá bán (và giá bán ở `ThuocEditPresenter`) không nhận dấu phân nhóm, nên “1.5” không còn bị hiểu thành 15.
- `ThongKeRepository`: dashboard dùng `AppConfig.SoNgayCanhBaoMacDinh` thay vì số 30 cố định; tìm hóa đơn khớp cả mã hiển thị `HDxxxxxx`.
- `NhapKhoPresenter`: đổi chú thích test case (xem trên).

## Việc còn lại
- Chưa được biên dịch/chạy thử trong môi trường tạo bản vá (cần Windows + .NET 10 SDK + SQL Server): hãy Build rồi chạy TC04, TC05, TC06, TC09–TC21 (các ca kiểm chứng phần đã sửa) và **TC26** (trang Phiếu nhập mới, chạy trước TC09).