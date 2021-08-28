using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class RenamedSectorAreatoLotArea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SectorArea",
                table: "PropertyObjects",
                newName: "LotArea");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LotArea",
                table: "PropertyObjects",
                newName: "SectorArea");
        }
    }
}
