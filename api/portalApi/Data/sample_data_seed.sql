-- Sample data seed for Marketplace app
-- Location: api/portalApi/Data/sample_data_seed.sql
-- Run this script against the target database (e.g. via SSMS or sqlcmd)

SET NOCOUNT ON;

BEGIN TRANSACTION;

-- Lookup values
PRINT 'Seeding LookupValues...';
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='Domain' AND [Value]='Access to Care')
    INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Access to Care');
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='Domain' AND [Value]='Quality')
    INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Quality');
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='Domain' AND [Value]='Strategy')
    INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Strategy');

IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='Division' AND [Value]='Greater Central Valley')
    INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater Central Valley');
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='Division' AND [Value]='Greater East Bay')
    INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater East Bay');

IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='ServiceLine' AND [Value]='Primary Care')
    INSERT INTO LookupValues([Type],[Value]) VALUES('ServiceLine','Primary Care');
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='ServiceLine' AND [Value]='Cardiology')
    INSERT INTO LookupValues([Type],[Value]) VALUES('ServiceLine','Cardiology');

IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='DataSource' AND [Value]='Power BI')
    INSERT INTO LookupValues([Type],[Value]) VALUES('DataSource','Power BI');
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='DataSource' AND [Value]='Epic')
    INSERT INTO LookupValues([Type],[Value]) VALUES('DataSource','Epic');

IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='AssetType' AND [Value]='Dashboard')
    INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Dashboard');
IF NOT EXISTS(SELECT 1 FROM LookupValues WHERE [Type]='AssetType' AND [Value]='Report')
    INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Report');

-- Single sample user
PRINT 'Seeding Users...';
DECLARE @UserId INT;
IF NOT EXISTS(SELECT 1 FROM Users WHERE UserPrincipalName = 'jdoe')
BEGIN
    INSERT INTO Users (UserPrincipalName, DisplayName, Email, UserType, CreatedAt)
    VALUES('jdoe','John Doe','jdoe@example.com','User', SYSUTCDATETIME());
    SET @UserId = SCOPE_IDENTITY();
END
ELSE
BEGIN
    SELECT @UserId = Id FROM Users WHERE UserPrincipalName = 'jdoe';
END

-- Sample items
PRINT 'Seeding Items...';
DECLARE @Item1 INT, @Item2 INT, @Item3 INT, @Item4 INT, @Item5 INT;

-- Item 1
IF NOT EXISTS(SELECT 1 FROM Items WHERE Url='https://dashboard.healthorg.com/patient-flow')
BEGIN
    INSERT INTO Items (Title, Description, Url, AssetTypesCsv, TagsCsv, Domain, Division, ServiceLine, DataSource, PrivacyPhi, DateAdded)
    VALUES (
        'Patient Flow Dashboard',
        'Visualizes patient wait times, throughput metrics, and bottlenecks across multiple departments.',
        'https://dashboard.healthorg.com/patient-flow',
        'Dashboard',
        'patient-flow,throughput,ed',
        'Access to Care', 'Greater Central Valley', 'Hospital', 'Power BI', 1, SYSUTCDATETIME()
    );
END
SELECT @Item1 = Id FROM Items WHERE Url='https://dashboard.healthorg.com/patient-flow';

-- Item 2
IF NOT EXISTS(SELECT 1 FROM Items WHERE Url='https://dashboard.healthorg.com/leadership-kpi')
BEGIN
    INSERT INTO Items (Title, Description, Url, AssetTypesCsv, TagsCsv, Domain, Division, ServiceLine, DataSource, PrivacyPhi, DateAdded)
    VALUES (
        'Leadership KPI Dashboard',
        'Executive-level dashboard summarizing financial, satisfaction, and strategic metrics.',
        'https://dashboard.healthorg.com/leadership-kpi',
        'Dashboard,Featured',
        'leadership,kpi,executive',
        'Strategy', 'Greater Sacramento', 'Oncology', 'Tableau', 0, DATEADD(day,-3,SYSUTCDATETIME())
    );
END
SELECT @Item2 = Id FROM Items WHERE Url='https://dashboard.healthorg.com/leadership-kpi';

-- Item 3
IF NOT EXISTS(SELECT 1 FROM Items WHERE Url='https://datamodel.healthorg.com/chronic')
BEGIN
    INSERT INTO Items (Title, Description, Url, AssetTypesCsv, TagsCsv, Domain, Division, ServiceLine, DataSource, PrivacyPhi, DateAdded)
    VALUES (
        'Chronic Disease Management Model',
        'Data model for chronic conditions enabling risk stratification and analytics.',
        'https://datamodel.healthorg.com/chronic',
        'Data Model',
        'chronic,disease,model',
        'Quality', 'Greater East Bay', 'Cardiology', 'Epic', 0, DATEADD(day,-6,SYSUTCDATETIME())
    );
END
SELECT @Item3 = Id FROM Items WHERE Url='https://datamodel.healthorg.com/chronic';

-- Item 4
IF NOT EXISTS(SELECT 1 FROM Items WHERE Url='https://reports.healthorg.com/scorecards')
BEGIN
    INSERT INTO Items (Title, Description, Url, AssetTypesCsv, TagsCsv, Domain, Division, ServiceLine, DataSource, PrivacyPhi, DateAdded)
    VALUES (
        'Monthly Provider Scorecards',
        'Scorecards for evaluating provider performance.',
        'https://reports.healthorg.com/scorecards',
        'Report',
        'scorecards,provider-performance',
        'Quality', 'Greater San Francisco', 'Primary Care', 'Power BI', 1, DATEADD(day,-8,SYSUTCDATETIME())
    );
END
SELECT @Item4 = Id FROM Items WHERE Url='https://reports.healthorg.com/scorecards';

-- Item 5
IF NOT EXISTS(SELECT 1 FROM Items WHERE Url='https://apps.healthorg.com/surgery-tracker')
BEGIN
    INSERT INTO Items (Title, Description, Url, AssetTypesCsv, TagsCsv, Domain, Division, ServiceLine, DataSource, PrivacyPhi, DateAdded)
    VALUES (
        'Surgical Volume Tracker',
        'Tracks surgical volume, cancellations and OR efficiency.',
        'https://apps.healthorg.com/surgery-tracker',
        'Application',
        'surgery,volume,tracker',
        'Access to Care', 'Greater Central Valley', 'Orthopedics', 'Web-Based', 1, DATEADD(day,-5,SYSUTCDATETIME())
    );
END
SELECT @Item5 = Id FROM Items WHERE Url='https://apps.healthorg.com/surgery-tracker';

-- User favorites
PRINT 'Seeding UserFavorites...';
IF NOT EXISTS(SELECT 1 FROM UserFavorites WHERE UserId=@UserId AND ItemId=@Item1)
    INSERT INTO UserFavorites (UserId, ItemId) VALUES (@UserId, @Item1);
IF NOT EXISTS(SELECT 1 FROM UserFavorites WHERE UserId=@UserId AND ItemId=@Item2)
    INSERT INTO UserFavorites (UserId, ItemId) VALUES (@UserId, @Item2);

-- User activity histories
PRINT 'Seeding histories...';
IF NOT EXISTS(SELECT 1 FROM UserAssetOpenHistories WHERE UserId=@UserId AND ItemId=@Item1)
    INSERT INTO UserAssetOpenHistories (UserId, ItemId, OpenedAt) VALUES (@UserId, @Item1, DATEADD(hour,-5,SYSUTCDATETIME()));

IF NOT EXISTS(SELECT 1 FROM UserSearchHistories WHERE UserId=@UserId AND Query='patient flow')
    INSERT INTO UserSearchHistories (UserId, Query, SearchedAt) VALUES (@UserId, 'patient flow', DATEADD(day,-1,SYSUTCDATETIME()));

IF NOT EXISTS(SELECT 1 FROM UserLoginHistories WHERE UserId=@UserId)
    INSERT INTO UserLoginHistories (UserId, LoggedInAt) VALUES (@UserId, DATEADD(day,-2,SYSUTCDATETIME()));

COMMIT TRANSACTION;

PRINT 'Sample data seeding complete.';

