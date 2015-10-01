CREATE PROCEDURE [dbo].[AddAccount]
    @address VARCHAR(1024),
    @issuerId INT,
    @currencyCode CHAR(3),
    @publicKey VARCHAR(MAX)
AS
BEGIN

    DECLARE @dt_now DATETIME = GETUTCDATE()
    DECLARE @vc_inserted_by VARCHAR(128) = SUSER_SNAME()

    INSERT INTO [dbo].[PaymentAccount]
    (
        [Address], [IssuerId], [CurrencyCode], [PublicKey], [InsertedDatetime], [InsertedBy]
    )
    VALUES
    (
        @address,
        @issuerId,
        @currencyCode,
        @publicKey,
        @dt_now,
        @vc_inserted_by
    )
    
    SELECT * FROM [dbo].[PaymentAccount]
    WHERE @address = [Address]

END
