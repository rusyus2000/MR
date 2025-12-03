using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedNewLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed OperatingEntity
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'OperatingEntity' AND [Value] = 'Unknown')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('OperatingEntity','Unknown');");

            // Seed RefreshFrequency
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Daily')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Daily');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Weekly')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Weekly');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Biweekly')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Biweekly');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = '2x/Month')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','2x/Month');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Monthly')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Monthly');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Quarterly')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Quarterly');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Annually')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Annually');");

            // Seed DataConsumer examples
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataConsumer' AND [Value] = 'Analysts')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('DataConsumer','Analysts');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataConsumer' AND [Value] = 'Leaders')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('DataConsumer','Leaders');");
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'DataConsumer' AND [Value] = 'Clinicians')
                                   INSERT INTO LookupValues([Type],[Value]) VALUES('DataConsumer','Clinicians');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
