CREATE TABLE [dbo].[ConfigCode]
(
	[ConfigCodeID] UNIQUEIDENTIFIER NOT NULL, 
	[ConfigCodeName] NVARCHAR(50) NOT NULL, 
	[Active] BIT NOT NULL DEFAULT 1, 
	[CreatedBy] UNIQUEIDENTIFIER NOT NULL, 
	[Created] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), 
	[DistrictID] UNIQUEIDENTIFIER NOT NULL, 
	[ConfigCodeAnnotation] NVARCHAR(80) NULL,
	PRIMARY KEY NONCLUSTERED ([ConfigCodeID] ASC),
	CONSTRAINT [FK_ConfigCode_ToDistrict] FOREIGN KEY ([DistrictID]) REFERENCES [dbo].[District] ([DistrictID]),
	CONSTRAINT [FK_ConfigCode_ToUser] FOREIGN KEY ([CreatedBy]) REFERENCES [User]([UserID])
)
GO

CREATE CLUSTERED INDEX [IX_ConfigCode_Created] ON [dbo].[ConfigCode] ([Created])
GO

CREATE INDEX [IX_ConfigCode_DistrictID] ON [dbo].[ConfigCode] (DistrictID)
GO

CREATE INDEX [IX_ConfigCode_ConfigCodeName] ON [dbo].[ConfigCode] ([ConfigCodeName])
GO