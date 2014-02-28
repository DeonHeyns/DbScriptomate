CREATE FUNCTION [dbo].[HasDbScriptBeenApplied]
(
	@ScriptNumber DECIMAL (18,1)
)
RETURNS BIT AS
BEGIN
	DECLARE @Result BIT = 0
	IF EXISTS (SELECT * FROM DbScripts WHERE ScriptNumber = @ScriptNumber)
		SET @Result = 1
	
	RETURN @Result
END