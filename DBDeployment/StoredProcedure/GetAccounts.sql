CREATE PROCEDURE [dbo].[GetAccounts]
    @issuerId int
AS
BEGIN
    
    SELECT * FROM [dbo].[PaymentAccount]
    WHERE [IssuerId] = @issuerId

END
