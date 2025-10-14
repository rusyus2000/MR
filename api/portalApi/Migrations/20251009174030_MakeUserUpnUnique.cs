using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserUpnUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users");

            // Consolidate duplicate UPNs: move dependent FKs to the kept row, then delete duplicates
            migrationBuilder.Sql(@"
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

");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users",
                column: "UserPrincipalName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users",
                column: "UserPrincipalName");
        }
    }
}
