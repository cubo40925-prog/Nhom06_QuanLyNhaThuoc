/* 02_InsertSampleData.sql - dữ liệu mẫu. Ngày tính theo ngày chạy script nên luôn có lô hết hạn / sắp hết hạn / sắp hết hàng.
   Tài khoản demo: duocsi / duocsi123 (Dược sĩ bán hàng), quanly / quanly123 (Quản lý kho). */
USE PharmaCareDB;
GO
SET NOCOUNT ON;
INSERT dbo.Users(Username,PasswordHash,FullName,Role) VALUES
('duocsi','PBKDF2$100000$QJoBXA57enaB52Qx2uxtOQ==$C1vkc+VIQUr7CuKJChTmbx/1F29JL0Ynk2MrVsm4KYk=',N'Nguyễn Thị Dược Sĩ','DUOCSI'),
('quanly','PBKDF2$100000$eKiDYwMpRJExiEVC9EA6cQ==$7Pvu5tz83/SKnnZyYVaKJBrdkIclHqa8hLNUBfRMwAk=',N'Trần Văn Quản Lý','QUANLYKHO');
INSERT dbo.Categories(CategoryName,Description) VALUES
(N'Giảm đau - hạ sốt',N'Thuốc giảm đau và hạ sốt'),(N'Tiêu hóa',N'Hỗ trợ hệ tiêu hóa'),(N'Vitamin - khoáng chất',N'Bổ sung vitamin và khoáng chất'),(N'Dị ứng',N'Thuốc chống dị ứng'),(N'Vật tư y tế',N'Vật tư y tế thông dụng');
INSERT dbo.Suppliers(SupplierName,Phone,Email,Address) VALUES
(N'Công ty Dược An Khang','02838220001','ankhang@example.com',N'TP. Hồ Chí Minh'),
(N'Nhà phân phối Minh Tâm','02838220002','minhtam@example.com',N'TP. Hồ Chí Minh'),
(N'Dược phẩm Việt Phúc','02838220003','vietphuc@example.com',N'Bình Dương');
INSERT dbo.Medicines(MedicineCode,MedicineName,CategoryID,ActiveIngredient,Unit,SellingPrice,IsPrescription) VALUES
('MED001',N'Paracetamol 500mg',1,N'Paracetamol',N'Viên',1000,0),
('MED002',N'Ibuprofen 200mg',1,N'Ibuprofen',N'Viên',1500,0),
('MED003',N'Oresol',2,N'Glucose + điện giải',N'Gói',5000,0),
('MED004',N'Men tiêu hóa',2,N'Probiotic',N'Gói',3000,0),
('MED005',N'Vitamin C 500mg',3,N'Acid ascorbic',N'Viên',1200,0),
('MED006',N'Vitamin tổng hợp',3,N'Multivitamin',N'Viên',2500,0),
('MED007',N'Loratadine 10mg',4,N'Loratadine',N'Viên',2000,0),
('MED008',N'Nước muối sinh lý 0.9%',5,N'Natri clorid',N'Chai',8000,0),
('MED009',N'Khẩu trang y tế',5,N'Vải không dệt',N'Hộp',25000,0),
('MED010',N'Amoxicillin 500mg',1,N'Amoxicillin',N'Viên',2500,1);
DECLARE @t DATE = CAST(GETDATE() AS date);
INSERT dbo.Batches(BatchCode,MedicineID,SupplierID,QuantityReceived,QuantityRemaining,CostPrice,SellingPrice,ReceivedDate,ExpiryDate) VALUES
('LOT-PA-001',1,1,500,380,600,1000,DATEADD(day,-30,@t),DATEADD(day,250,@t)),
('LOT-IB-001',2,1,300,210,900,1500,DATEADD(day,-30,@t),DATEADD(day,400,@t)),
('LOT-OR-001',3,2,200,120,3000,5000,DATEADD(day,-50,@t),DATEADD(day,18,@t)),     -- sắp hết hạn
('LOT-MEN-001',4,2,250,8,1800,3000,DATEADD(day,-40,@t),DATEADD(day,500,@t)),     -- sắp hết hàng
('LOT-VC-001',5,3,400,270,700,1200,DATEADD(day,-10,@t),DATEADD(day,600,@t)),
('LOT-VT-001',6,3,200,140,1500,2500,DATEADD(day,-15,@t),DATEADD(day,15,@t)),     -- sắp hết hạn
('LOT-LOR-001',7,1,300,180,1000,2000,DATEADD(day,-25,@t),DATEADD(day,350,@t)),
('LOT-NS-001',8,2,150,95,5000,8000,DATEADD(day,-10,@t),DATEADD(day,800,@t)),
('LOT-KT-001',9,2,100,40,18000,25000,DATEADD(day,-8,@t),DATEADD(day,900,@t)),
('LOT-AM-001',10,1,180,100,1500,2500,DATEADD(day,-5,@t),DATEADD(day,300,@t)),
('LOT-AM-000',10,1,120,40,1400,2500,DATEADD(day,-400,@t),DATEADD(day,-20,@t)),   -- ĐÃ hết hạn
('LOT-PA-000',1,1,200,25,550,1000,DATEADD(day,-380,@t),DATEADD(day,-35,@t));     -- ĐÃ hết hạn
-- Phiếu nhập mẫu (lịch sử nhập kho): mỗi lô ở trên có phiếu nhập tương ứng, ngày phiếu = ngày nhập lô. Phiếu 5 nhập 2 lô cùng lúc.
INSERT dbo.ReceiptNotes(ReceiptDate,SupplierID,CreatedBy,TotalAmount) VALUES
(CAST(DATEADD(day,-400,@t) AS DATETIME2),1,'quanly',168000),
(CAST(DATEADD(day,-380,@t) AS DATETIME2),1,'quanly',110000),
(CAST(DATEADD(day,-50,@t) AS DATETIME2),2,'quanly',600000),
(CAST(DATEADD(day,-40,@t) AS DATETIME2),2,'quanly',450000),
(CAST(DATEADD(day,-30,@t) AS DATETIME2),1,'quanly',570000),
(CAST(DATEADD(day,-25,@t) AS DATETIME2),1,'quanly',300000),
(CAST(DATEADD(day,-15,@t) AS DATETIME2),3,'quanly',300000),
(CAST(DATEADD(day,-10,@t) AS DATETIME2),3,'quanly',280000),
(CAST(DATEADD(day,-10,@t) AS DATETIME2),2,'quanly',750000),
(CAST(DATEADD(day,-8,@t) AS DATETIME2),2,'quanly',1800000),
(CAST(DATEADD(day,-5,@t) AS DATETIME2),1,'quanly',270000);
INSERT dbo.ReceiptLines(ReceiptID,BatchID,Quantity,CostPrice)
SELECT v.r, b.BatchID, b.QuantityReceived, b.CostPrice
FROM (VALUES (1,11),(2,12),(3,3),(4,4),(5,1),(5,2),(6,7),(7,6),(8,5),(9,8),(10,9),(11,10)) v(r,bid) JOIN dbo.Batches b ON b.BatchID = v.bid;
INSERT dbo.Invoices(InvoiceDate,CustomerName,TotalAmount,CreatedBy) VALUES
(DATEADD(hour,-2,SYSDATETIME()),N'Khách lẻ',18000,'duocsi'),
(DATEADD(day,-1,SYSDATETIME()),N'Nguyễn Văn A',28000,'duocsi');
INSERT dbo.InvoiceLines(InvoiceID,BatchID,Quantity,UnitPrice) VALUES(1,1,8,1000),(1,3,2,5000),(2,5,10,1200),(2,8,2,8000);
GO
