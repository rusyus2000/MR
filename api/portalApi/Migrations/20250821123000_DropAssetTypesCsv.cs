using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    [DbContext(typeof(SutterAnalyticsApi.Data.AppDbContext))]
    [Migration("20250821123000_DropAssetTypesCsv")]
    public partial class DropAssetTypesCsv : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'AssetTypesCsv' AND Object_ID = Object_ID(N'Items')) BEGIN ALTER TABLE Items DROP COLUMN [AssetTypesCsv] END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate as nullable string
            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'AssetTypesCsv' AND Object_ID = Object_ID(N'Items')) BEGIN ALTER TABLE Items ADD [AssetTypesCsv] nvarchar(max) NULL END");
        }
    }
}

