CREATE TABLE [dbo].[PaymentAccount]
(
    [Address] VARCHAR(1024) NOT NULL PRIMARY KEY,
    [IssuerId] INT NOT NULL,
    [CurrencyCode] CHAR(3) NOT NULL,
    [PublicKey] VARCHAR(MAX) NOT NULL,
    [InsertedDatetime] DATETIME NOT NULL,
    [InsertedBy] NVARCHAR(64) NOT NULL,
    [UpdatedDatetime] DATETIME NULL,
    [UpdatedBy] NVARCHAR(64) NULL,
)
