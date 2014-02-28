DECLARE @ScriptNumber DECIMAL(18,1)		= {0}
DECLARE @ScriptType VARCHAR (3)			= '{1}' -- DDL or DML
DECLARE @Author  VARCHAR (50)			= '{2}' -- Initials
DECLARE @Description VARCHAR (500)		= '{3}' -- Short description

DECLARE @FixedScriptNumber VARCHAR (10) =  REPLACE(CONVERT (VARCHAR(20), @ScriptNumber), ',', '.') -- Fix the decimal place char on machines with the wrong regional settings.
DECLARE @FileName VARCHAR (255) = @FixedScriptNumber + '.' + @Author + '.' + @ScriptType + '.' + @Description  + '.' + 'sql'

IF dbo.HasDbScriptBeenApplied (@ScriptNumber) = 1
BEGIN
	DECLARE @Message varchar(1000)
	SET @Message = 'This script has already been applied to this database. Probable file name = ' + @FileName
	RAISERROR(@Message, 16, -1)
END

BEGIN TRAN t1
EXEC dbo.LogDbScript @ScriptNumber, @Author, @Description
SELECT @FileName AS FileName

	--=========================================================================
	--	Put your script for a stored procedure below the GO
	--	Including the GO to execute the Stored procedure
GO

--=========================================================================
IF @@ERROR != 0
BEGIN
	ROLLBACK TRAN t1;
	RAISERROR('Script failed', 16, -1)
END
ELSE
BEGIN
	COMMIT TRAN t1
END
