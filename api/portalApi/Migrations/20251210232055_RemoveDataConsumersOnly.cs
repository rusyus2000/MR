using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDataConsumersOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatasetKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false),
                    ToCreate = table.Column<int>(type: "int", nullable: false),
                    ToUpdate = table.Column<int>(type: "int", nullable: false),
                    Unchanged = table.Column<int>(type: "int", nullable: false),
                    Conflicts = table.Column<int>(type: "int", nullable: false),
                    Errors = table.Column<int>(type: "int", nullable: false),
                    PreviewJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LookupValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPrincipalName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssetTypeId = table.Column<int>(type: "int", nullable: true),
                    Featured = table.Column<bool>(type: "bit", nullable: true),
                    DomainId = table.Column<int>(type: "int", nullable: true),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    ServiceLineId = table.Column<int>(type: "int", nullable: true),
                    DataSourceId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    PrivacyPhi = table.Column<bool>(type: "bit", nullable: true),
                    PrivacyPii = table.Column<bool>(type: "bit", nullable: true),
                    HasRls = table.Column<bool>(type: "bit", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperatingEntityId = table.Column<int>(type: "int", nullable: true),
                    ExecutiveSponsorId = table.Column<int>(type: "int", nullable: true),
                    RefreshFrequencyId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dependencies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultAdGroupNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductGroup = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProductStatusNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataConsumers = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TechDeliveryManager = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegulatoryComplianceContractual = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BiPlatform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DbServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DbDataMart = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DatabaseTable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceRep = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DataSecurityClassification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessGroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccessGroupDn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AutomationClassification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserVisibilityString = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserVisibilityNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EpicSecurityGroupTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KeepLongTerm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PotentialToConsolidateId = table.Column<int>(type: "int", nullable: true),
                    PotentialToAutomateId = table.Column<int>(type: "int", nullable: true),
                    SponsorBusinessValueId = table.Column<int>(type: "int", nullable: true),
                    MustDo2025Id = table.Column<int>(type: "int", nullable: true),
                    DevelopmentEffortId = table.Column<int>(type: "int", nullable: true),
                    EstimatedDevHoursId = table.Column<int>(type: "int", nullable: true),
                    ResourcesDevelopmentId = table.Column<int>(type: "int", nullable: true),
                    ResourcesAnalystsId = table.Column<int>(type: "int", nullable: true),
                    ResourcesPlatformId = table.Column<int>(type: "int", nullable: true),
                    ResourcesDataEngineeringId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_DevelopmentEffortId",
                        column: x => x.DevelopmentEffortId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_DomainId",
                        column: x => x.DomainId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_EstimatedDevHoursId",
                        column: x => x.EstimatedDevHoursId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_MustDo2025Id",
                        column: x => x.MustDo2025Id,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_OperatingEntityId",
                        column: x => x.OperatingEntityId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_PotentialToAutomateId",
                        column: x => x.PotentialToAutomateId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_PotentialToConsolidateId",
                        column: x => x.PotentialToConsolidateId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_RefreshFrequencyId",
                        column: x => x.RefreshFrequencyId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_ResourcesAnalystsId",
                        column: x => x.ResourcesAnalystsId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_ResourcesDataEngineeringId",
                        column: x => x.ResourcesDataEngineeringId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_ResourcesDevelopmentId",
                        column: x => x.ResourcesDevelopmentId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_ResourcesPlatformId",
                        column: x => x.ResourcesPlatformId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_ServiceLineId",
                        column: x => x.ServiceLineId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_SponsorBusinessValueId",
                        column: x => x.SponsorBusinessValueId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_LookupValues_StatusId",
                        column: x => x.StatusId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Owners_ExecutiveSponsorId",
                        column: x => x.ExecutiveSponsorId,
                        principalTable: "Owners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Items_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserLoginHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoggedInAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLoginHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSearchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearchHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSearchHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemTags",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTags", x => new { x.ItemId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ItemTags_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAssetOpenHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssetOpenHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAssetOpenHistories_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAssetOpenHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavorites",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavorites", x => new { x.UserId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_UserFavorites_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavorites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_Token",
                table: "ImportJobs",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_AssetTypeId",
                table: "Items",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DataSourceId",
                table: "Items",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DateAdded",
                table: "Items",
                column: "DateAdded");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DevelopmentEffortId",
                table: "Items",
                column: "DevelopmentEffortId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DivisionId",
                table: "Items",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DomainId",
                table: "Items",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_EstimatedDevHoursId",
                table: "Items",
                column: "EstimatedDevHoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ExecutiveSponsorId",
                table: "Items",
                column: "ExecutiveSponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Featured",
                table: "Items",
                column: "Featured");

            migrationBuilder.CreateIndex(
                name: "IX_Items_HasRls",
                table: "Items",
                column: "HasRls");

            migrationBuilder.CreateIndex(
                name: "IX_Items_MustDo2025Id",
                table: "Items",
                column: "MustDo2025Id");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OperatingEntityId",
                table: "Items",
                column: "OperatingEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OwnerId",
                table: "Items",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PotentialToAutomateId",
                table: "Items",
                column: "PotentialToAutomateId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PotentialToConsolidateId",
                table: "Items",
                column: "PotentialToConsolidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PrivacyPhi",
                table: "Items",
                column: "PrivacyPhi");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PrivacyPii",
                table: "Items",
                column: "PrivacyPii");

            migrationBuilder.CreateIndex(
                name: "IX_Items_RefreshFrequencyId",
                table: "Items",
                column: "RefreshFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ResourcesAnalystsId",
                table: "Items",
                column: "ResourcesAnalystsId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ResourcesDataEngineeringId",
                table: "Items",
                column: "ResourcesDataEngineeringId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ResourcesDevelopmentId",
                table: "Items",
                column: "ResourcesDevelopmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ResourcesPlatformId",
                table: "Items",
                column: "ResourcesPlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ServiceLineId",
                table: "Items",
                column: "ServiceLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SponsorBusinessValueId",
                table: "Items",
                column: "SponsorBusinessValueId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_StatusId",
                table: "Items",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemTags_TagId",
                table: "ItemTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_LookupValues_Type_Value",
                table: "LookupValues",
                columns: new[] { "Type", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Value",
                table: "Tags",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssetOpenHistories_ItemId",
                table: "UserAssetOpenHistories",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssetOpenHistories_UserId",
                table: "UserAssetOpenHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_ItemId",
                table: "UserFavorites",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginHistories_UserId",
                table: "UserLoginHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users",
                column: "UserPrincipalName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchHistories_UserId",
                table: "UserSearchHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropTable(
                name: "ItemTags");

            migrationBuilder.DropTable(
                name: "UserAssetOpenHistories");

            migrationBuilder.DropTable(
                name: "UserFavorites");

            migrationBuilder.DropTable(
                name: "UserLoginHistories");

            migrationBuilder.DropTable(
                name: "UserSearchHistories");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LookupValues");

            migrationBuilder.DropTable(
                name: "Owners");
        }
    }
}
