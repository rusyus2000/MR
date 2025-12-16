IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [Items] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [AssetTypesCsv] nvarchar(max) NOT NULL,
        [TagsCsv] nvarchar(max) NOT NULL,
        [Domain] nvarchar(max) NOT NULL,
        [Division] nvarchar(max) NOT NULL,
        [ServiceLine] nvarchar(max) NOT NULL,
        [DataSource] nvarchar(max) NOT NULL,
        [PrivacyPhi] bit NOT NULL,
        [DateAdded] datetime2 NOT NULL,
        CONSTRAINT [PK_Items] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [LookupValues] (
        [Id] int NOT NULL IDENTITY,
        [Type] nvarchar(max) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_LookupValues] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [UserPrincipalName] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [UserType] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [UserAssetOpenHistories] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ItemId] int NOT NULL,
        [OpenedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserAssetOpenHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserAssetOpenHistories_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserAssetOpenHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [UserFavorites] (
        [UserId] int NOT NULL,
        [ItemId] int NOT NULL,
        CONSTRAINT [PK_UserFavorites] PRIMARY KEY ([UserId], [ItemId]),
        CONSTRAINT [FK_UserFavorites_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserFavorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [UserLoginHistories] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [LoggedInAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserLoginHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserLoginHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE TABLE [UserSearchHistories] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Query] nvarchar(max) NOT NULL,
        [SearchedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserSearchHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserSearchHistories_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserAssetOpenHistories_ItemId] ON [UserAssetOpenHistories] ([ItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserAssetOpenHistories_UserId] ON [UserAssetOpenHistories] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserFavorites_ItemId] ON [UserFavorites] ([ItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserLoginHistories_UserId] ON [UserLoginHistories] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserSearchHistories_UserId] ON [UserSearchHistories] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250815203844_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250815203844_InitialCreate', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250816_NormalizeTags'
)
BEGIN
    CREATE TABLE [Tags] (
        [Id] int NOT NULL IDENTITY,
        [Value] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_Tags] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250816_NormalizeTags'
)
BEGIN
    CREATE TABLE [ItemTags] (
        [ItemId] int NOT NULL,
        [TagId] int NOT NULL,
        CONSTRAINT [PK_ItemTags] PRIMARY KEY ([ItemId], [TagId]),
        CONSTRAINT [FK_ItemTags_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ItemTags_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250816_NormalizeTags'
)
BEGIN
    CREATE INDEX [IX_ItemTags_TagId] ON [ItemTags] ([TagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250816_NormalizeTags'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Tags_Value] ON [Tags] ([Value]) WHERE [Value] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250816_NormalizeTags'
)
BEGIN

                    INSERT INTO Tags(Value)
                    SELECT DISTINCT LTRIM(RTRIM(s.value))
                    FROM Items
                    CROSS APPLY STRING_SPLIT(Items.TagsCsv, ',') s
                    WHERE Items.TagsCsv IS NOT NULL AND Items.TagsCsv <> ''
                        AND LTRIM(RTRIM(s.value)) <> ''
                        AND NOT EXISTS(SELECT 1 FROM Tags t WHERE t.Value = LTRIM(RTRIM(s.value)));

                    INSERT INTO ItemTags(ItemId, TagId)
                    SELECT i.Id, t.Id
                    FROM Items i
                    CROSS APPLY STRING_SPLIT(i.TagsCsv, ',') s
                    JOIN Tags t ON t.Value = LTRIM(RTRIM(s.value))
                    WHERE i.TagsCsv IS NOT NULL AND i.TagsCsv <> '' AND LTRIM(RTRIM(s.value)) <> ''
                    AND NOT EXISTS(SELECT 1 FROM ItemTags it WHERE it.ItemId = i.Id AND it.TagId = t.Id);

                    -- Drop the old TagsCsv column now that tags are normalized
                    ALTER TABLE Items DROP COLUMN TagsCsv;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250816_NormalizeTags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250816_NormalizeTags', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819221905_FinalNormalizeTags'
)
BEGIN
    DROP INDEX [IX_Tags_Value] ON [Tags];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819221905_FinalNormalizeTags'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tags]') AND [c].[name] = N'Value');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Tags] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Tags] ALTER COLUMN [Value] nvarchar(max) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250819221905_FinalNormalizeTags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250819221905_FinalNormalizeTags', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD [AssetTypeId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD [DataSourceId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD [DivisionId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD [DomainId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD [ServiceLineId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    CREATE INDEX [IX_Items_AssetTypeId] ON [Items] ([AssetTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    CREATE INDEX [IX_Items_DataSourceId] ON [Items] ([DataSourceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    CREATE INDEX [IX_Items_DivisionId] ON [Items] ([DivisionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    CREATE INDEX [IX_Items_DomainId] ON [Items] ([DomainId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    CREATE INDEX [IX_Items_ServiceLineId] ON [Items] ([ServiceLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_AssetTypeId] FOREIGN KEY ([AssetTypeId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_DataSourceId] FOREIGN KEY ([DataSourceId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_DivisionId] FOREIGN KEY ([DivisionId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_DomainId] FOREIGN KEY ([DomainId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_ServiceLineId] FOREIGN KEY ([ServiceLineId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820153055_AddLookupFks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250820153055_AddLookupFks', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'UserPrincipalName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Users] ALTER COLUMN [UserPrincipalName] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tags]') AND [c].[name] = N'Value');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Tags] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Tags] ALTER COLUMN [Value] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LookupValues]') AND [c].[name] = N'Value');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [LookupValues] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [LookupValues] ALTER COLUMN [Value] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LookupValues]') AND [c].[name] = N'Type');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [LookupValues] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [LookupValues] ALTER COLUMN [Type] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    CREATE INDEX [IX_Users_UserPrincipalName] ON [Users] ([UserPrincipalName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    CREATE INDEX [IX_Tags_Value] ON [Tags] ([Value]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    CREATE INDEX [IX_LookupValues_Type_Value] ON [LookupValues] ([Type], [Value]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    CREATE INDEX [IX_Items_DateAdded] ON [Items] ([DateAdded]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    CREATE INDEX [IX_Items_Featured] ON [Items] ([Featured]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    CREATE INDEX [IX_Items_PrivacyPhi] ON [Items] ([PrivacyPhi]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250820223633_AddIndexesForFilters'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250820223633_AddIndexesForFilters', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821120000_PopulateLookupIdsAndDropTextCols'
)
BEGIN

    INSERT INTO LookupValues (Type, Value)
    SELECT DISTINCT 'Domain', [Domain] FROM Items i
    WHERE [Domain] IS NOT NULL AND [Domain] <> ''
      AND NOT EXISTS(SELECT 1 FROM LookupValues lv WHERE lv.[Type] = 'Domain' AND lv.[Value] = i.[Domain]);

    INSERT INTO LookupValues (Type, Value)
    SELECT DISTINCT 'Division', [Division] FROM Items i
    WHERE [Division] IS NOT NULL AND [Division] <> ''
      AND NOT EXISTS(SELECT 1 FROM LookupValues lv WHERE lv.[Type] = 'Division' AND lv.[Value] = i.[Division]);

    INSERT INTO LookupValues (Type, Value)
    SELECT DISTINCT 'ServiceLine', [ServiceLine] FROM Items i
    WHERE [ServiceLine] IS NOT NULL AND [ServiceLine] <> ''
      AND NOT EXISTS(SELECT 1 FROM LookupValues lv WHERE lv.[Type] = 'ServiceLine' AND lv.[Value] = i.[ServiceLine]);

    INSERT INTO LookupValues (Type, Value)
    SELECT DISTINCT 'DataSource', [DataSource] FROM Items i
    WHERE [DataSource] IS NOT NULL AND [DataSource] <> ''
      AND NOT EXISTS(SELECT 1 FROM LookupValues lv WHERE lv.[Type] = 'DataSource' AND lv.[Value] = i.[DataSource]);

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821120000_PopulateLookupIdsAndDropTextCols'
)
BEGIN

    UPDATE i SET DomainId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'Domain' AND i.[Domain] = lv.[Value]
    WHERE i.[Domain] IS NOT NULL;

    UPDATE i SET DivisionId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'Division' AND i.[Division] = lv.[Value]
    WHERE i.[Division] IS NOT NULL;

    UPDATE i SET ServiceLineId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'ServiceLine' AND i.[ServiceLine] = lv.[Value]
    WHERE i.[ServiceLine] IS NOT NULL;

    UPDATE i SET DataSourceId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'DataSource' AND i.[DataSource] = lv.[Value]
    WHERE i.[DataSource] IS NOT NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821120000_PopulateLookupIdsAndDropTextCols'
)
BEGIN

    ALTER TABLE Items DROP COLUMN [Domain];
    ALTER TABLE Items DROP COLUMN [Division];
    ALTER TABLE Items DROP COLUMN [ServiceLine];
    ALTER TABLE Items DROP COLUMN [DataSource];

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821120000_PopulateLookupIdsAndDropTextCols'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250821120000_PopulateLookupIdsAndDropTextCols', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821121000_PopulateAssetTypeFk'
)
BEGIN

    INSERT INTO LookupValues ([Type], [Value])
    SELECT DISTINCT 'AssetType', TRIM(value)
    FROM Items
    CROSS APPLY STRING_SPLIT(AssetTypesCsv, ',')
    WHERE TRIM(value) <> ''
      AND NOT EXISTS(
        SELECT 1 FROM LookupValues lv WHERE lv.[Type] = 'AssetType' AND lv.[Value] = TRIM(value)
      );

    -- Set AssetTypeId on items by matching any token in AssetTypesCsv to a LookupValue
    UPDATE i
    SET AssetTypeId = lv.Id
    FROM Items i
    CROSS APPLY STRING_SPLIT(i.AssetTypesCsv, ',') s
    INNER JOIN LookupValues lv ON lv.[Type] = 'AssetType' AND lv.[Value] = TRIM(s.value)
    WHERE i.AssetTypeId IS NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821121000_PopulateAssetTypeFk'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250821121000_PopulateAssetTypeFk', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821123000_DropAssetTypesCsv'
)
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'AssetTypesCsv' AND Object_ID = Object_ID(N'Items')) BEGIN ALTER TABLE Items DROP COLUMN [AssetTypesCsv] END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821123000_DropAssetTypesCsv'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250821123000_DropAssetTypesCsv', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821130000_AddFeaturedColumn'
)
BEGIN
    ALTER TABLE [Items] ADD [Featured] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250821130000_AddFeaturedColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250821130000_AddFeaturedColumn', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    ALTER TABLE [Items] ADD [OwnerId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    ALTER TABLE [Items] ADD [StatusId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    CREATE TABLE [Owners] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Owners] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    CREATE INDEX [IX_Items_OwnerId] ON [Items] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    CREATE INDEX [IX_Items_StatusId] ON [Items] ([StatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_Owners_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Owners] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250902223614_AddOwnerAndStatus'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250902223614_AddOwnerAndStatus', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009174030_MakeUserUpnUnique'
)
BEGIN
    DROP INDEX [IX_Users_UserPrincipalName] ON [Users];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009174030_MakeUserUpnUnique'
)
BEGIN

    IF OBJECT_ID('tempdb..#dups') IS NOT NULL DROP TABLE #dups;
    CREATE TABLE #dups (Id INT NOT NULL, KeepId INT NOT NULL);

    INSERT INTO #dups(Id, KeepId)
    SELECT Id, KeepId FROM (
      SELECT Id, MIN(Id) OVER (PARTITION BY UserPrincipalName) AS KeepId
      FROM Users
    ) t
    WHERE Id <> KeepId;

    -- Repoint favorites
    UPDATE uf SET UserId = d.KeepId
    FROM UserFavorites uf
    JOIN #dups d ON uf.UserId = d.Id;
    -- Repoint asset open history
    UPDATE uo SET UserId = d.KeepId
    FROM UserAssetOpenHistories uo
    JOIN #dups d ON uo.UserId = d.Id;
    -- Repoint login history
    UPDATE ul SET UserId = d.KeepId
    FROM UserLoginHistories ul
    JOIN #dups d ON ul.UserId = d.Id;
    -- Repoint search history
    UPDATE us SET UserId = d.KeepId
    FROM UserSearchHistories us
    JOIN #dups d ON us.UserId = d.Id;

    -- Delete duplicate user rows
    DELETE FROM Users WHERE Id IN (SELECT Id FROM #dups);

    DROP TABLE #dups;

    -- Ensure any residual duplicates (case-only) are unique by suffixing
    ;WITH d2 AS (
      SELECT Id, UserPrincipalName,
             ROW_NUMBER() OVER (PARTITION BY UserPrincipalName ORDER BY Id) AS rn
      FROM Users
    )
    UPDATE Users SET UserPrincipalName = UserPrincipalName + ':' + CAST(Id AS nvarchar(20))
    WHERE Id IN (SELECT Id FROM d2 WHERE rn > 1);


END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009174030_MakeUserUpnUnique'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_UserPrincipalName] ON [Users] ([UserPrincipalName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251009174030_MakeUserUpnUnique'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251009174030_MakeUserUpnUnique', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Active - Legacy')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Active - Legacy');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Active - Approved Standard in Prod')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Active - Approved Standard in Prod');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Active - Legal Retained')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Active - Legal Retained');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Decommissioned')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Decommissioned');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Not Started')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Not Started');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'On-Hold')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','On-Hold');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Pending Decommision')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Pending Decommision');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Unknown')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Unknown');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Released to Production')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Released to Production');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Acquisition or in Development')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Acquisition or in Development');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'In Pilot')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','In Pilot');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'In Review')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','In Review');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'In Development')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','In Development');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Access')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Access');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Business Operations (e.g. Digital, Marketing, Legal)')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Business Operations (e.g. Digital, Marketing, Legal)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Clinical Operations')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Clinical Operations');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Division')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Division');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Finance')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Finance');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Foundation')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Foundation');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Growth & Strategy')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Growth & Strategy');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Hospital')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Hospital');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Independent Practice Association (IPA)')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Independent Practice Association (IPA)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Insured Products')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Insured Products');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Medical Group')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Medical Group');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Patient Experience')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Patient Experience');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'People & Workforce')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','People & Workforce');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Quality ')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Quality ');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Service Lines')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Service Lines');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'API')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','API');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Application')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Application');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Code')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Code');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Dashboard')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Dashboard');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Data Extract')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Data Extract');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'ETL Data Pipeline')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','ETL Data Pipeline');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'ML Model')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','ML Model');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Patient Registry')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Patient Registry');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Report')
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Report');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Greater Central Coast')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater Central Coast');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Greater Central Valley')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater Central Valley');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Greater East Bay')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater East Bay');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Greater Sacramento')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater Sacramento');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Greater San Francisco (including North Bay)')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater San Francisco (including North Bay)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Greater Silicon Valley')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Greater Silicon Valley');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251014211003_SeedStandardLookups'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251014211003_SeedStandardLookups', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015211908_ImportInfra'
)
BEGIN
    ALTER TABLE [Items] ADD [ContentHash] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015211908_ImportInfra'
)
BEGIN
    ALTER TABLE [Items] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015211908_ImportInfra'
)
BEGIN
    ALTER TABLE [Items] ADD [UpdatedBy] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015211908_ImportInfra'
)
BEGIN
    CREATE TABLE [ImportJobs] (
        [Id] int NOT NULL IDENTITY,
        [Token] nvarchar(450) NOT NULL,
        [Source] nvarchar(max) NULL,
        [DatasetKey] nvarchar(max) NULL,
        [UploadedBy] nvarchar(max) NULL,
        [UploadedAt] datetime2 NOT NULL,
        [FileName] nvarchar(max) NULL,
        [Mode] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [Total] int NOT NULL,
        [ToCreate] int NOT NULL,
        [ToUpdate] int NOT NULL,
        [Unchanged] int NOT NULL,
        [Conflicts] int NOT NULL,
        [Errors] int NOT NULL,
        [PreviewJson] nvarchar(max) NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        CONSTRAINT [PK_ImportJobs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015211908_ImportInfra'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ImportJobs_Token] ON [ImportJobs] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015211908_ImportInfra'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251015211908_ImportInfra', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [DefaultAdGroupNames] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [Dependencies] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [ExecutiveSponsorId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [HasRls] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [LastModifiedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [OperatingEntityId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [PrivacyPii] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD [RefreshFrequencyId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE TABLE [ItemDataConsumers] (
        [ItemId] int NOT NULL,
        [DataConsumerId] int NOT NULL,
        CONSTRAINT [PK_ItemDataConsumers] PRIMARY KEY ([ItemId], [DataConsumerId]),
        CONSTRAINT [FK_ItemDataConsumers_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [Items] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ItemDataConsumers_LookupValues_DataConsumerId] FOREIGN KEY ([DataConsumerId]) REFERENCES [LookupValues] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE INDEX [IX_Items_ExecutiveSponsorId] ON [Items] ([ExecutiveSponsorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE INDEX [IX_Items_HasRls] ON [Items] ([HasRls]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE INDEX [IX_Items_OperatingEntityId] ON [Items] ([OperatingEntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE INDEX [IX_Items_PrivacyPii] ON [Items] ([PrivacyPii]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE INDEX [IX_Items_RefreshFrequencyId] ON [Items] ([RefreshFrequencyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    CREATE INDEX [IX_ItemDataConsumers_DataConsumerId] ON [ItemDataConsumers] ([DataConsumerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_OperatingEntityId] FOREIGN KEY ([OperatingEntityId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_RefreshFrequencyId] FOREIGN KEY ([RefreshFrequencyId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_Owners_ExecutiveSponsorId] FOREIGN KEY ([ExecutiveSponsorId]) REFERENCES [Owners] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028214950_AddGovernanceAndConsumers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251028214950_AddGovernanceAndConsumers', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'OperatingEntity' AND [Value] = 'Unknown')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('OperatingEntity','Unknown');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Daily')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Daily');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Weekly')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Weekly');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Biweekly')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Biweekly');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = '2x/Month')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','2x/Month');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Monthly')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Monthly');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Quarterly')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Quarterly');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Annually')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Annually');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataConsumer' AND [Value] = 'Analysts')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('DataConsumer','Analysts');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataConsumer' AND [Value] = 'Leaders')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('DataConsumer','Leaders');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataConsumer' AND [Value] = 'Clinicians')
                                       INSERT INTO LookupValues([Type],[Value]) VALUES('DataConsumer','Clinicians');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251028215638_SeedNewLookups'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251028215638_SeedNewLookups', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [ProductGroup] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [ProductStatusNotes] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [DataConsumersText] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [TechDeliveryManager] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [RegulatoryComplianceContractual] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [BiPlatform] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [DbServer] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [DbDataMart] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [DatabaseTable] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [SourceRep] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [DataSecurityClassification] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [AccessGroupName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [AccessGroupDn] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [AutomationClassification] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [UserVisibilityString] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [UserVisibilityNumber] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [EpicSecurityGroupTag] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    ALTER TABLE [Items] ADD [KeepLongTerm] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202121500_AddCatalogFreeText'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251202121500_AddCatalogFreeText', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202124800_MakeBooleanFieldsNullable'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'PrivacyPhi');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Items] ALTER COLUMN [PrivacyPhi] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202124800_MakeBooleanFieldsNullable'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'PrivacyPii');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Items] ALTER COLUMN [PrivacyPii] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202124800_MakeBooleanFieldsNullable'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'HasRls');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Items] ALTER COLUMN [HasRls] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202124800_MakeBooleanFieldsNullable'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Items]') AND [c].[name] = N'Featured');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Items] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Items] ALTER COLUMN [Featured] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202124800_MakeBooleanFieldsNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251202124800_MakeBooleanFieldsNullable', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'AssetType' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('AssetType','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ServiceLine' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('ServiceLine','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataSource' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('DataSource','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Status' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('Status','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'OperatingEntity' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('OperatingEntity','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Missing Data')
    BEGIN
        INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Missing Data');
    END;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN

    UPDATE i SET AssetTypeId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'AssetType' AND lv.[Value] = 'Missing Data'
    WHERE i.AssetTypeId IS NULL;

    UPDATE i SET DomainId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'Domain' AND lv.[Value] = 'Missing Data'
    WHERE i.DomainId IS NULL;

    UPDATE i SET DivisionId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'Division' AND lv.[Value] = 'Missing Data'
    WHERE i.DivisionId IS NULL;

    UPDATE i SET ServiceLineId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'ServiceLine' AND lv.[Value] = 'Missing Data'
    WHERE i.ServiceLineId IS NULL;

    UPDATE i SET DataSourceId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'DataSource' AND lv.[Value] = 'Missing Data'
    WHERE i.DataSourceId IS NULL;

    UPDATE i SET StatusId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'Status' AND lv.[Value] = 'Missing Data'
    WHERE i.StatusId IS NULL;

    UPDATE i SET OperatingEntityId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'OperatingEntity' AND lv.[Value] = 'Missing Data'
    WHERE i.OperatingEntityId IS NULL;

    UPDATE i SET RefreshFrequencyId = lv.Id
    FROM Items i
    JOIN LookupValues lv ON lv.[Type] = 'RefreshFrequency' AND lv.[Value] = 'Missing Data'
    WHERE i.RefreshFrequencyId IS NULL;


END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202133000_FillMissingLookupsWithMissingData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251202133000_FillMissingLookupsWithMissingData', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [PotentialToConsolidateId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [PotentialToAutomateId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [SponsorBusinessValueId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [MustDo2025Id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [DevelopmentEffortId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [EstimatedDevHoursId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [ResourcesDevelopmentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [ResourcesAnalystsId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [ResourcesPlatformId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD [ResourcesDataEngineeringId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_PotentialToConsolidateId] ON [Items] ([PotentialToConsolidateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_PotentialToAutomateId] ON [Items] ([PotentialToAutomateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_SponsorBusinessValueId] ON [Items] ([SponsorBusinessValueId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_MustDo2025Id] ON [Items] ([MustDo2025Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_DevelopmentEffortId] ON [Items] ([DevelopmentEffortId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_EstimatedDevHoursId] ON [Items] ([EstimatedDevHoursId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_ResourcesDevelopmentId] ON [Items] ([ResourcesDevelopmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_ResourcesAnalystsId] ON [Items] ([ResourcesAnalystsId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_ResourcesPlatformId] ON [Items] ([ResourcesPlatformId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    CREATE INDEX [IX_Items_ResourcesDataEngineeringId] ON [Items] ([ResourcesDataEngineeringId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_PotentialToConsolidateId] FOREIGN KEY ([PotentialToConsolidateId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_PotentialToAutomateId] FOREIGN KEY ([PotentialToAutomateId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_SponsorBusinessValueId] FOREIGN KEY ([SponsorBusinessValueId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_MustDo2025Id] FOREIGN KEY ([MustDo2025Id]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_DevelopmentEffortId] FOREIGN KEY ([DevelopmentEffortId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_EstimatedDevHoursId] FOREIGN KEY ([EstimatedDevHoursId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_ResourcesDevelopmentId] FOREIGN KEY ([ResourcesDevelopmentId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_ResourcesAnalystsId] FOREIGN KEY ([ResourcesAnalystsId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_ResourcesPlatformId] FOREIGN KEY ([ResourcesPlatformId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    ALTER TABLE [Items] ADD CONSTRAINT [FK_Items_LookupValues_ResourcesDataEngineeringId] FOREIGN KEY ([ResourcesDataEngineeringId]) REFERENCES [LookupValues] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'PotentialToConsolidate' AND [Value] = 'Yes')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('PotentialToConsolidate','Yes'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'PotentialToConsolidate' AND [Value] = 'No')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('PotentialToConsolidate','No'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'PotentialToAutomate' AND [Value] = 'Yes')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('PotentialToAutomate','Yes'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'PotentialToAutomate' AND [Value] = 'No')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('PotentialToAutomate','No'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'SponsorBusinessValue' AND [Value] = 'Low')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('SponsorBusinessValue','Low'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'SponsorBusinessValue' AND [Value] = 'Medium')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('SponsorBusinessValue','Medium'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'SponsorBusinessValue' AND [Value] = 'High')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('SponsorBusinessValue','High'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'MustDo2025' AND [Value] = 'Yes')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('MustDo2025','Yes'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'MustDo2025' AND [Value] = 'No')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('MustDo2025','No'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DevelopmentEffort' AND [Value] = 'Low')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('DevelopmentEffort','Low'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DevelopmentEffort' AND [Value] = 'Medium')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('DevelopmentEffort','Medium'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DevelopmentEffort' AND [Value] = 'High')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('DevelopmentEffort','High'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'EstimatedDevHours' AND [Value] = '40')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('EstimatedDevHours','40'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'EstimatedDevHours' AND [Value] = '100')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('EstimatedDevHours','100'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'EstimatedDevHours' AND [Value] = '200')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('EstimatedDevHours','200'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'EstimatedDevHours' AND [Value] = '300')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('EstimatedDevHours','300'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'EstimatedDevHours' AND [Value] = '500')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('EstimatedDevHours','500'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'EstimatedDevHours' AND [Value] = '1000')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('EstimatedDevHours','1000'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ResourcesDevelopment' AND [Value] = 'TBD')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('ResourcesDevelopment','TBD'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ResourcesAnalysts' AND [Value] = 'TBD')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('ResourcesAnalysts','TBD'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ResourcesPlatform' AND [Value] = 'TBD')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('ResourcesPlatform','TBD'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ResourcesDataEngineering' AND [Value] = 'TBD')
    BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('ResourcesDataEngineering','TBD'); END;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202134500_AddNewItemLookupFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251202134500_AddNewItemLookupFields', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202140000_FillMissingOwnersAndSponsors'
)
BEGIN

    IF NOT EXISTS (SELECT 1 FROM Owners WHERE [Name] = 'Missing Data' AND [Email] = 'missing@example.com')
    BEGIN
        INSERT INTO Owners([Name],[Email]) VALUES('Missing Data','missing@example.com');
    END;

    DECLARE @missingId INT;
    SELECT @missingId = Id FROM Owners WHERE [Name] = 'Missing Data' AND [Email] = 'missing@example.com';

    -- Set missing OwnerId
    UPDATE i SET OwnerId = @missingId
    FROM Items i
    WHERE i.OwnerId IS NULL;

    -- Set missing ExecutiveSponsorId
    UPDATE i SET ExecutiveSponsorId = @missingId
    FROM Items i
    WHERE i.ExecutiveSponsorId IS NULL;


END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251202140000_FillMissingOwnersAndSponsors'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251202140000_FillMissingOwnersAndSponsors', N'9.0.8');
END;

COMMIT;
GO

