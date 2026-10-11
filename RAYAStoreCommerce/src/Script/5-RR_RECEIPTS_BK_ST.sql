Create or Alter PROCEDURE [ext].[RR_RECEIPTS_BK_ST]
    @nvc_UUID		   NVARCHAR(250),
    @nvc_PREVIOUSUUID    NVARCHAR(250),
    @nvc_REFERENCEUUID  NVARCHAR(250),
    @nvc_DEVSERIALNUMBER  NVARCHAR(100),
	@nvc_Receipt_URL  NVARCHAR(1000),
    @nvc_TransactionId  NVARCHAR(40),
	@nvc_ReceiptId  NVARCHAR(18),
	@nvc_Store	 NVARCHAR(10),
@nvc_Json_Receipt NVARCHAR(max),
@nvc_DataAreaId  NVARCHAR(4),
@nvc_Receipt_DateTime nvarchar(50)

AS
BEGIN
    SET NOCOUNT ON
    
	IF Not EXISTS    
    (
    SELECT 1    
    FROM [ext].[RR_RECEIPTS_BK_TB]    
    WHERE [TRANSACTIONID] = @nvc_TransactionId  
        AND [RECEIPTID] = @nvc_ReceiptId    
    )    
    BEGIN
      
				INSERT INTO [ext].[RR_RECEIPTS_BK_TB]
           ([TRANSACTIONID]
           ,[RECEIPTID]
           ,[STORE]
           ,[RECEIPT_URL]
           ,[DEVSERIALNUMBER]
           ,[REFERENCEUUID]
           ,[PREVIOUSUUID]
           ,[UUID],[Json_Receipt],[DataAreaId],[Receipt_DateTime])
     VALUES
           (@nvc_TransactionId,@nvc_ReceiptId,@nvc_Store,@nvc_Receipt_URL,@nvc_DEVSERIALNUMBER,@nvc_REFERENCEUUID,@nvc_PREVIOUSUUID,@nvc_UUID,@nvc_Json_Receipt,@nvc_DataAreaId,@nvc_Receipt_DateTime)
        
		 END
   
END;
