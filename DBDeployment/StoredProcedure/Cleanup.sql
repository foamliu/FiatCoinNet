CREATE PROCEDURE [dbo].[Cleanup]
AS
BEGIN

    --TOOD: testing only, very dangerous!!!
    DELETE FROM [dbo].[PaymentAccount]
    DELETE FROM [dbo].[PaymentTransaction]

END
