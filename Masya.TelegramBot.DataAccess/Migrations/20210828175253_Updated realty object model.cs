using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Updatedrealtyobjectmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_PropertyObjects_PropertyObjectId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_PropertyObjects_PropertyObjectId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "PropertyObjects");

            migrationBuilder.AddColumn<string>(
                name: "ImportUrl",
                table: "Agencies",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RealtyObjects",
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
                    TotalArea = table.Column<float>(type: "real", nullable: false),
                    LivingSpace = table.Column<float>(type: "real", nullable: false),
                    KitchenSpace = table.Column<float>(type: "real", nullable: false),
                    LotArea = table.Column<float>(type: "real", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: true),
                    TotalFloors = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    MailingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RealtyObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_DirectoryItems_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_DirectoryItems_StateId",
                        column: x => x.StateId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_DirectoryItems_StreetId",
                        column: x => x.StreetId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_DirectoryItems_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_DirectoryItems_WallMaterialId",
                        column: x => x.WallMaterialId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RealtyObjects_Users_AgentId1",
                        column: x => x.AgentId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_AgentId1",
                table: "RealtyObjects",
                column: "AgentId1");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_CategoryId",
                table: "RealtyObjects",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_DistrictId",
                table: "RealtyObjects",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_StateId",
                table: "RealtyObjects",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_StreetId",
                table: "RealtyObjects",
                column: "StreetId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_TypeId",
                table: "RealtyObjects",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_WallMaterialId",
                table: "RealtyObjects",
                column: "WallMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_RealtyObjects_PropertyObjectId",
                table: "Favorites",
                column: "PropertyObjectId",
                principalTable: "RealtyObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_RealtyObjects_PropertyObjectId",
                table: "Reports",
                column: "PropertyObjectId",
                principalTable: "RealtyObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_RealtyObjects_PropertyObjectId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_RealtyObjects_PropertyObjectId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "RealtyObjects");

            migrationBuilder.DropColumn(
                name: "ImportUrl",
                table: "Agencies");

            migrationBuilder.CreateTable(
                name: "PropertyObjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    AgentId1 = table.Column<long>(type: "bigint", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: true),
                    InternalId = table.Column<int>(type: "int", nullable: true),
                    KitchenArea = table.Column<int>(type: "int", nullable: false),
                    LiveArea = table.Column<int>(type: "int", nullable: false),
                    LotArea = table.Column<int>(type: "int", nullable: false),
                    MailingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StateId = table.Column<int>(type: "int", nullable: true),
                    StreetId = table.Column<int>(type: "int", nullable: true),
                    TotalArea = table.Column<int>(type: "int", nullable: false),
                    TotalFloors = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    WallMaterialId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_PropertyObjects_Users_AgentId1",
                        column: x => x.AgentId1,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_PropertyObjects_PropertyObjectId",
                table: "Favorites",
                column: "PropertyObjectId",
                principalTable: "PropertyObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_PropertyObjects_PropertyObjectId",
                table: "Reports",
                column: "PropertyObjectId",
                principalTable: "PropertyObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
