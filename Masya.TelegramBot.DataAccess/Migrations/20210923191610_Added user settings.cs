using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Addedusersettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserSettingsId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

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

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinPrice = table.Column<int>(type: "int", nullable: false),
                    MaxPrice = table.Column<int>(type: "int", nullable: false),
                    MinFloor = table.Column<int>(type: "int", nullable: false),
                    MaxFloor = table.Column<int>(type: "int", nullable: false),
                    MaxRoomsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserSettings_UserSettingsId",
                table: "Users",
                column: "UserSettingsId",
                principalTable: "UserSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_UserSettings_UserSettingsId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_DirectoryItems_UserSettings_UserSettingsId",
                table: "DirectoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserSettings_UserSettingsId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserSettings");

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
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserSettingsId",
                table: "DirectoryItems");

            migrationBuilder.DropColumn(
                name: "UserSettingsId",
                table: "Categories");
        }
    }
}
