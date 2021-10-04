using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Addedliststocategoriesanddiritems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_UserSettings_UserSettingsId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryItems_UserSettings_UserSettingsId",
                table: "DirectoryItems");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserSettingsId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_DirectoryItems_UserSettingsId",
                table: "DirectoryItems");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UserSettingsId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserSettingsId",
                table: "DirectoryItems");

            migrationBuilder.DropColumn(
                name: "UserSettingsId",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "CategoryUserSettings",
                columns: table => new
                {
                    SelectedCategoriesId = table.Column<int>(type: "int", nullable: false),
                    UserSettingsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryUserSettings", x => new { x.SelectedCategoriesId, x.UserSettingsId });
                    table.ForeignKey(
                        name: "FK_CategoryUserSettings_Categories_SelectedCategoriesId",
                        column: x => x.SelectedCategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryUserSettings_UserSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DirectoryItemUserSettings",
                columns: table => new
                {
                    SelectedRegionsId = table.Column<int>(type: "int", nullable: false),
                    UserSettingsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectoryItemUserSettings", x => new { x.SelectedRegionsId, x.UserSettingsId });
                    table.ForeignKey(
                        name: "FK_DirectoryItemUserSettings_DirectoryItems_SelectedRegionsId",
                        column: x => x.SelectedRegionsId,
                        principalTable: "DirectoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectoryItemUserSettings_UserSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserSettingsId",
                table: "Users",
                column: "UserSettingsId",
                unique: true,
                filter: "[UserSettingsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryUserSettings_UserSettingsId",
                table: "CategoryUserSettings",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryItemUserSettings_UserSettingsId",
                table: "DirectoryItemUserSettings",
                column: "UserSettingsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryUserSettings");

            migrationBuilder.DropTable(
                name: "DirectoryItemUserSettings");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserSettingsId",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "UserSettingsId",
                table: "DirectoryItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserSettingsId",
                table: "Categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserSettingsId",
                table: "Users",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_DirectoryItems_UserSettingsId",
                table: "DirectoryItems",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserSettingsId",
                table: "Categories",
                column: "UserSettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_UserSettings_UserSettingsId",
                table: "Categories",
                column: "UserSettingsId",
                principalTable: "UserSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DirectoryItems_UserSettings_UserSettingsId",
                table: "DirectoryItems",
                column: "UserSettingsId",
                principalTable: "UserSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
