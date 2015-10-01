CREATE PROCEDURE [dbo].[GetAccount]
    @address VARCHAR(1024)
AS
BEGIN
    
    SELECT * FROM [dbo].[PaymentAccount]
    WHERE @address = [Address]

END
