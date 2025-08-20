using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SutterAnalyticsApi.Data;


#nullable disable

namespace portalApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250821120000_PopulateLookupIdsAndDropTextCols")]
    public partial class PopulateLookupIdsAndDropTextCols : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert missing lookup values for Domain, Division, ServiceLine, DataSource
            migrationBuilder.Sql(@"
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
");

            // Populate FK columns on Items from the lookup values
            migrationBuilder.Sql(@"
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
");

            // Drop the now-redundant text columns from Items
            migrationBuilder.Sql(@"
ALTER TABLE Items DROP COLUMN [Domain];
ALTER TABLE Items DROP COLUMN [Division];
ALTER TABLE Items DROP COLUMN [ServiceLine];
ALTER TABLE Items DROP COLUMN [DataSource];
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate text columns (nullable) and populate from lookup FKs
            migrationBuilder.Sql(@"
ALTER TABLE Items ADD [Domain] nvarchar(max) NULL;
ALTER TABLE Items ADD [Division] nvarchar(max) NULL;
ALTER TABLE Items ADD [ServiceLine] nvarchar(max) NULL;
ALTER TABLE Items ADD [DataSource] nvarchar(max) NULL;

UPDATE i SET [Domain] = lv.[Value]
FROM Items i
JOIN LookupValues lv ON i.DomainId = lv.Id;

UPDATE i SET [Division] = lv.[Value]
FROM Items i
JOIN LookupValues lv ON i.DivisionId = lv.Id;

UPDATE i SET [ServiceLine] = lv.[Value]
FROM Items i
JOIN LookupValues lv ON i.ServiceLineId = lv.Id;

UPDATE i SET [DataSource] = lv.[Value]
FROM Items i
JOIN LookupValues lv ON i.DataSourceId = lv.Id;
");
        }
    }
}
