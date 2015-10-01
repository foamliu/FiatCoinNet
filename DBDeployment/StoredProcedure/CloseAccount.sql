CREATE PROCEDURE [dbo].[CloseAccount]
    @address VARCHAR(1024)
AS
BEGIN
    
    --TOOD: soft delete
    DELETE FROM [dbo].[PaymentAccount]
    WHERE @address = [Address]

END
