CREATE PROCEDURE [dbo].[GetTransactions]
    @issuerId INT,
    @address VARCHAR(1024)
AS
BEGIN

    SELECT * FROM [dbo].PaymentTransaction
    WHERE [IssuerId] = @issuerId 
        AND ([Source] = @address OR [Dest] = @address)

END
