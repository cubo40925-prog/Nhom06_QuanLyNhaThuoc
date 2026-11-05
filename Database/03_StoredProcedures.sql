/* 03_StoredProcedures.sql */
USE PharmaCareDB;
GO
-- Lô đã hết hạn / sắp hết hạn trong @DaysAhead ngày / sắp hết hàng (<= @LowStock). Cột đặt tên tiếng Việt để hiển thị trực tiếp.
CREATE OR ALTER PROCEDURE dbo.sp_Lo_CanhBao @DaysAhead INT = 30, @LowStock INT = 10
AS
BEGIN
 SET NOCOUNT ON;
 DECLARE @t DATE = CAST(GETDATE() AS date);
 SELECT b.BatchCode AS [Mã lô], m.MedicineName AS [Tên thuốc], b.ExpiryDate AS [Hạn dùng],
        DATEDIFF(day,@t,b.ExpiryDate) AS [Còn (ngày)], b.QuantityRemaining AS [Tồn],
        CASE WHEN b.ExpiryDate < @t THEN N'Đã hết hạn'
             WHEN b.ExpiryDate <= DATEADD(day,@DaysAhead,@t) THEN N'Sắp hết hạn'
             ELSE N'Sắp hết hàng' END AS [Trạng thái]
 FROM dbo.Batches b JOIN dbo.Medicines m ON m.MedicineID = b.MedicineID
 WHERE b.QuantityRemaining > 0 AND (b.ExpiryDate <= DATEADD(day,@DaysAhead,@t) OR b.QuantityRemaining <= @LowStock)
 ORDER BY b.ExpiryDate, m.MedicineName;
END
GO
-- Tồn kho theo từng lô, xếp theo hạn dùng (thứ tự FEFO). @MedicineID = NULL: tất cả thuốc.
CREATE OR ALTER PROCEDURE dbo.sp_Lo_TonTheoLo @MedicineID INT = NULL
AS
BEGIN
 SET NOCOUNT ON;
 DECLARE @t DATE = CAST(GETDATE() AS date);
 SELECT b.BatchCode AS [Mã lô], m.MedicineName AS [Tên thuốc], b.ReceivedDate AS [Ngày nhập], b.ExpiryDate AS [Hạn dùng],
        b.QuantityRemaining AS [Tồn], b.CostPrice AS [Giá nhập], b.SellingPrice AS [Giá bán],
        CASE WHEN b.ExpiryDate < @t THEN N'Đã hết hạn'
             WHEN b.ExpiryDate <= DATEADD(day,30,@t) THEN N'Sắp hết hạn'
             ELSE N'Bình thường' END AS [Trạng thái]
 FROM dbo.Batches b JOIN dbo.Medicines m ON m.MedicineID = b.MedicineID
 WHERE b.QuantityRemaining > 0 AND (@MedicineID IS NULL OR b.MedicineID = @MedicineID)
 ORDER BY m.MedicineName, b.ExpiryDate, b.BatchID;
END
GO
-- Nhập lô (UPSERT): mã lô đã có thì cộng dồn số lượng (giữ nguyên giá bán của lô), chưa có thì thêm mới. Gọi trong SqlTransaction của ứng dụng.
CREATE OR ALTER PROCEDURE dbo.sp_Lo_Nhap
 @BatchCode VARCHAR(40), @MedicineID INT, @SupplierID INT = NULL, @ReceivedDate DATE, @ExpiryDate DATE,
 @Quantity INT, @CostPrice DECIMAL(18,2), @SellingPrice DECIMAL(18,2), @BatchID INT OUTPUT
AS
BEGIN
 SET NOCOUNT ON;
 IF @Quantity <= 0 THROW 50010, N'Số lượng nhập phải lớn hơn 0.', 1;
 IF @ExpiryDate <= CAST(GETDATE() AS date) THROW 50011, N'Hạn sử dụng phải sau ngày hôm nay.', 1;
 IF @SellingPrice <= 0 THROW 50013, N'Giá bán phải lớn hơn 0.', 1;

 DECLARE @oldMed INT, @oldExp DATE;
 SELECT @BatchID = BatchID, @oldMed = MedicineID, @oldExp = ExpiryDate
 FROM dbo.Batches WITH (UPDLOCK, HOLDLOCK) WHERE BatchCode = @BatchCode;

 IF @BatchID IS NOT NULL
 BEGIN
  IF @oldMed <> @MedicineID THROW 50014, N'Mã lô này đã thuộc về thuốc khác.', 1;
  IF @oldExp <> @ExpiryDate THROW 50012, N'Mã lô đã tồn tại với hạn dùng khác. Hãy kiểm tra lại mã lô hoặc hạn dùng.', 1;
  -- Giá bán của lô GIỮ NGUYÊN (không đổi giá hàng đã có trên kệ); giá nhập = bình quân gia quyền theo số lượng nhập.
  -- Trong UPDATE, vế phải luôn dùng giá trị CŨ của dòng nên QuantityReceived ở công thức giá là số lượng trước khi cộng.
  UPDATE dbo.Batches
     SET CostPrice = CAST((CostPrice * QuantityReceived + @CostPrice * @Quantity) / (QuantityReceived + @Quantity) AS DECIMAL(18,2)),
         QuantityReceived = QuantityReceived + @Quantity, QuantityRemaining = QuantityRemaining + @Quantity,
         SupplierID = ISNULL(@SupplierID, SupplierID)
   WHERE BatchID = @BatchID;
 END
 ELSE
 BEGIN
  INSERT dbo.Batches(BatchCode,MedicineID,SupplierID,QuantityReceived,QuantityRemaining,CostPrice,SellingPrice,ReceivedDate,ExpiryDate)
  VALUES(@BatchCode,@MedicineID,@SupplierID,@Quantity,@Quantity,@CostPrice,@SellingPrice,@ReceivedDate,@ExpiryDate);
  SET @BatchID = SCOPE_IDENTITY();
 END
END
GO
