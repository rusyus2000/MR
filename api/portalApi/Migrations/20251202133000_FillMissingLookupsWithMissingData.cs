using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SutterAnalyticsApi.Data;

namespace portalApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20251202133000_FillMissingLookupsWithMissingData")]
    public partial class FillMissingLookupsWithMissingData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure 'Missing Data' exists for each lookup type we use on Items
            var types = new []
            {
                "AssetType","Domain","Division","ServiceLine","DataSource","Status","OperatingEntity","RefreshFrequency"
            };

            foreach (var t in types)
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = '{t}' AND [Value] = 'Missing Data')
BEGIN
    INSERT INTO LookupValues([Type],[Value]) VALUES('{t}','Missing Data');
END;
");
            }

            // Update Items FK columns where NULL -> set to the 'Missing Data' id for that type
            migrationBuilder.Sql(@"
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

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: we do not revert data back to NULL
        }
    }
}

