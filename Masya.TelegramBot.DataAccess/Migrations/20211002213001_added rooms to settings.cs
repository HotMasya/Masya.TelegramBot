using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class addedroomstosettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRoomsCount",
                table: "UserSettings");

            migrationBuilder.CreateTable(
                name: "RoomUserSettings",
                columns: table => new
                {
                    RoomsId = table.Column<int>(type: "int", nullable: false),
                    UserSettingsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomUserSettings", x => new { x.RoomsId, x.UserSettingsId });
                    table.ForeignKey(
                        name: "FK_RoomUserSettings_Rooms_RoomsId",
                        column: x => x.RoomsId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomUserSettings_UserSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomUserSettings_UserSettingsId",
                table: "RoomUserSettings",
                column: "UserSettingsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomUserSettings");

            migrationBuilder.AddColumn<int>(
                name: "MaxRoomsCount",
                table: "UserSettings",
                type: "int",
                nullable: true);
        }
    }
}
