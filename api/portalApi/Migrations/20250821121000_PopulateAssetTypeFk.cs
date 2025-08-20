using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    [DbContext(typeof(SutterAnalyticsApi.Data.AppDbContext))]
    [Migration("20250821121000_PopulateAssetTypeFk")]
    public partial class PopulateAssetTypeFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert distinct asset type values from the AssetTypesCsv column
            // (split comma-separated values) into LookupValues where missing.
            migrationBuilder.Sql(@"
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
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort rollback: clear AssetTypeId on items (do not delete lookup rows)
            migrationBuilder.Sql("UPDATE Items SET AssetTypeId = NULL WHERE AssetTypeId IS NOT NULL;");
        }
    }
}

