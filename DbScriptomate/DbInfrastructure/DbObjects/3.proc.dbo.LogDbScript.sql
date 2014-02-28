CREATE PROC [dbo].[LogDbScript]
	@ScriptNumber decimal(18, 1),
	@Author varchar(50),
	@Description varchar(500)
AS
BEGIN
	INSERT INTO dbo.DbScripts
	(
		ScriptNumber,
		Author,
		Description,
		DateApplied,
		AppliedToDb,
		AppliedByLogin,
		AppliedFromMachine,
		SqlServerVersion
	)
	VALUES
	(
		@ScriptNumber,
		@Author,
		@Description,
		SYSDATETIMEOFFSET(),
		DB_NAME(),
		SUSER_NAME(),
		HOST_NAME(),
		@@Version
	)
END