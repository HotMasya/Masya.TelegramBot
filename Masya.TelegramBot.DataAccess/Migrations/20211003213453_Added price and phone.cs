using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Addedpriceandphone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rooms",
                table: "RealtyObjects",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rooms",
                table: "RealtyObjects");
        }
    }
}
