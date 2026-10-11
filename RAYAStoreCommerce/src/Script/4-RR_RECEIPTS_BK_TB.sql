IF (SELECT OBJECT_ID('[ext].[RR_RECEIPTS_BK_TB]')) IS not NULL
    DROP table [ext].[RR_RECEIPTS_BK_TB]
GO

CREATE TABLE [ext].[RR_RECEIPTS_BK_TB](
	[TRANSACTIONID] [nvarchar](44) NULL,
	[RECEIPTID] [nvarchar](18) NULL,
	[STORE] [nvarchar](10) NULL,
	[RECEIPT_URL] [nvarchar](1000) NULL,
	[DEVSERIALNUMBER] [nvarchar](100) NULL,
	[REFERENCEUUID] [nvarchar](250) NULL,
	[PREVIOUSUUID] [nvarchar](250) NULL,
	[UUID] [nvarchar](250) NULL,
	[Json_Receipt] nvarchar(max) NULL,
	[Receipt_DateTime] nvarchar(50),
	[REPLICATIONCOUNTERFROMORIGIN] [int] IDENTITY(1,1) NOT NULL,
	[ROWVERSION] [timestamp] NOT NULL,
	[DATAAREAID] [nvarchar](4) NOT NULL
) ON [PRIMARY]
GO