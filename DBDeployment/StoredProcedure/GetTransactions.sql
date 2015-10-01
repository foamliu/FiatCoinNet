CREATE PROCEDURE [dbo].[GetTransactions]
    @address VARCHAR(1024)
AS
BEGIN

    SELECT * FROM [dbo].PaymentTransaction
    WHERE [Source] = @address OR [Dest] = @address

END
