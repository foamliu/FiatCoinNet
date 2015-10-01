﻿CREATE TABLE [dbo].[PaymentTransaction]
(
    [Source] VARCHAR(1024) NOT NULL,
    [Dest] VARCHAR(1024) NOT NULL,
    [Amount] MONEY NOT NULL,
    [CurrencyCode] CHAR(3),
    [MemoData] NVARCHAR(MAX),
    [InsertedDatetime] DATETIME NOT NULL,
    [InsertedBy] NVARCHAR(64) NOT NULL,
    PRIMARY KEY ([Source], [Dest], [InsertedDatetime])
)
