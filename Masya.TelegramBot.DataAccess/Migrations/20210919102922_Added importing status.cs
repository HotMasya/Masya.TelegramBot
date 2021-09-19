using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Addedimportingstatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImporting",
                table: "BotSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImporting",
                table: "BotSettings");
        }
    }
}
