using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SutterAnalyticsApi.Data;

namespace portalApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20251202140000_FillMissingOwnersAndSponsors")]
    public partial class FillMissingOwnersAndSponsors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a generic "Missing Data" person in Owners if not present
            migrationBuilder.Sql(@"
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

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No data rollback
        }
    }
}

