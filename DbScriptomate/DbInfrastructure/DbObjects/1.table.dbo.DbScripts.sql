/*scripts table*/
CREATE TABLE [dbo].[DbScripts](
	[ScriptNumber] [decimal](18, 1) NOT NULL,
	[Author] [varchar](50) NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[AppliedToDb] [varchar](100) NOT NULL,
	[DateApplied] [datetimeoffset](4) NOT NULL,
	[AppliedByLogin] [varchar](250) NOT NULL,
	[AppliedFromMachine] [varchar](250) NOT NULL,
	[SqlServerVersion] [varchar](300) NOT NULL,
 CONSTRAINT [PK_DbScripts] PRIMARY KEY CLUSTERED 
(
	[ScriptNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[DbScripts] ADD  CONSTRAINT [DF_DbScripts_DateApplied]  DEFAULT (sysdatetimeoffset()) FOR [DateApplied]

