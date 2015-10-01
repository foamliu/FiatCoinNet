CREATE PROCEDURE [dbo].[AddTransaction]
    @source VARCHAR(1024),
    @dest VARCHAR(1024),
    @amount MONEY,
    @currencyCode CHAR(3),
    @memoData NVARCHAR(MAX)
AS
BEGIN
    DECLARE @dt_now DATETIME = GETUTCDATE()
    DECLARE @vc_inserted_by VARCHAR(128) = SUSER_SNAME()

    INSERT INTO [dbo].[PaymentTransaction]
    (
        [Source], [Dest], [Amount], [CurrencyCode], [MemoData], [InsertedDatetime], [InsertedBy]
    )
    VALUES
    (
        @source,
        @dest,
        @amount,
        @currencyCode,
        @memoData,
        @dt_now,
        @vc_inserted_by
    )

    SELECT * FROM [dbo].[PaymentTransaction]
    WHERE [Source] = @source
        AND [Dest] = @dest
        AND [InsertedDatetime] = @dt_now

END
