CREATE PROCEDURE [dbo].[GetAccount]
    @issuerId INT,
    @address VARCHAR(1024)
AS
BEGIN
    
    SELECT * FROM [dbo].[PaymentAccount]
    WHERE @issuerId = [IssuerId]
        AND @address = [Address]

END
