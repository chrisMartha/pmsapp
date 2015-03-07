CREATE TABLE [dbo].[LicenseRequest](
	[LicenseRequestID] [uniqueidentifier] NOT NULL,
	[PrevLicenseRequestID] [uniqueidentifier] NULL,
	[DistrictID] [uniqueidentifier] NULL,
	[SchoolID] [uniqueidentifier] NULL,
	[ConfigCode] [nvarchar](50) NOT NULL,
	[WifiBSSID] [char](17) NOT NULL,
	[LicenseRequestTypeID] [int] NOT NULL,
	[DeviceID] [uniqueidentifier] NOT NULL,
	[UserID] [uniqueidentifier] NOT NULL,
	[RequestDateTime] [datetime] NOT NULL,
	[Response] [nvarchar](50) NULL,
	[ResponseDateTime] [datetime] NULL,
	[LocationID] [nvarchar](50) NULL,
	[LocationName] [nvarchar](50) NULL,
	[LearningContentQueued] [int] NULL,
	[Created] [datetime2](7) NOT NULL DEFAULT (sysutcdatetime()),
	PRIMARY KEY NONCLUSTERED ([LicenseRequestID] ASC),
)
GO

CREATE CLUSTERED INDEX [IX_LicenseRequest_Created] ON [dbo].[LicenseRequest] ([Created])
GO
