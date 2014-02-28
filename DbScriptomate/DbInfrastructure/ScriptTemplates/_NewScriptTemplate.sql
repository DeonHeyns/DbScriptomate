DECLARE @ScriptNumber DECIMAL(18,1)		= {0}
DECLARE @ScriptType VARCHAR (3)			= '{1}' -- DDL or DML
DECLARE @Author  VARCHAR (50)			= '{2}' -- Initials
DECLARE @Description VARCHAR (500)		= '{3}' -- Short description

DECLARE @FixedScriptNumber VARCHAR (10) =  REPLACE(CONVERT (VARCHAR(20), @ScriptNumber), ',', '.') -- Fix the decimal place char on machines with the wrong regional settings.
DECLARE @FileName VARCHAR (255) = @FixedScriptNumber + '.' + @Author + '.' + @ScriptType + '.' + @Description  + '.' + 'sql'

IF dbo.HasDbScriptBeenApplied (@ScriptNumber) = 1
BEGIN
	PRINT 'This script has already been applied to this database. Probable file name = ' + @FileName
	RETURN
END

BEGIN TRY
	BEGIN TRAN t1
	--=========================================================================

	--  Put your script here
	--  (You cannot have GO statements in this block - they will break the transaction)
	--	So, Don't use this template for stored procs. Use the following one in stead: _NewScriptTemplate_StoredProcedures.sql
	

	--=========================================================================
	EXEC dbo.LogDbScript @ScriptNumber, @Author, @Description
	COMMIT TRAN t1;
END TRY
BEGIN CATCH
	ROLLBACK TRAN t1;
	THROW; -- Rethrow the original error after rolling back transaction.
END CATCH
SELECT @FileName AS FileName
