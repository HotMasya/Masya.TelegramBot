using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    DateOfUnblock = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    IsRegWithoutAdmin = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SuperType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Directories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyId = table.Column<int>(type: "int", nullable: true),
                    TelegramAccountId = table.Column<long>(type: "bigint", nullable: false),
                    TelegramLogin = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TelegramAvatar = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TelegramFirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TelegramLastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TelegramPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastCalledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    BlockReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsBlockedByBot = table.Column<bool>(type: "bit", nullable: true),
                    IsIgnored = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "References",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_References", x => x.Id);
                    table.ForeignKey(
                        name: "FK_References_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DirectoryId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DirectoryItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryItems_Directories_DirectoryId",
                        column: x => x.DirectoryId,
                        principalTable: "Directories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropertyObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalId = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    StreetId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    WallMaterialId = table.Column<int>(type: "int", nullable: true),
                    StateId = table.Column<int>(type: "int", nullable: true),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    AgentId1 = table.Column<long>(type: "bigint", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TotalArea = table.Column<int>(type: "int", nullable: false),
                    LiveArea = table.Column<int>(type: "int", nullable: false),
                    KitchenArea = table.Column<int>(type: "int", nullable: false),
                    SectorArea = table.Column<int>(type: "int", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: true),
                    TotalFloors = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    MailingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_DirectoryItems_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_DirectoryItems_StateId",
                        column: x => x.StateId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_DirectoryItems_StreetId",
                        column: x => x.StreetId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_DirectoryItems_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_DirectoryItems_WallMaterialId",
                        column: x => x.WallMaterialId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyObjects_User_AgentId1",
                        column: x => x.AgentId1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserId1 = table.Column<long>(type: "bigint", nullable: true),
                    PropertyObjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_PropertyObjects_PropertyObjectId",
                        column: x => x.PropertyObjectId,
                        principalTable: "PropertyObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_User_UserId1",
                        column: x => x.UserId1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyObjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserId1 = table.Column<long>(type: "bigint", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_PropertyObjects_PropertyObjectId",
                        column: x => x.PropertyObjectId,
                        principalTable: "PropertyObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_User_UserId1",
                        column: x => x.UserId1,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryItems_CategoryId",
                table: "DirectoryItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryItems_DirectoryId",
                table: "DirectoryItems",
                column: "DirectoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_PropertyObjectId",
                table: "Favorites",
                column: "PropertyObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId1",
                table: "Favorites",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_AgentId1",
                table: "PropertyObjects",
                column: "AgentId1");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_CategoryId",
                table: "PropertyObjects",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_DistrictId",
                table: "PropertyObjects",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_StateId",
                table: "PropertyObjects",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_StreetId",
                table: "PropertyObjects",
                column: "StreetId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_TypeId",
                table: "PropertyObjects",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyObjects_WallMaterialId",
                table: "PropertyObjects",
                column: "WallMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_References_AgencyId",
                table: "References",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PropertyObjectId",
                table: "Reports",
                column: "PropertyObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId1",
                table: "Reports",
                column: "UserId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "References");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "PropertyObjects");

            migrationBuilder.DropTable(
                name: "DirectoryItems");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Directories");
        }
    }
}
