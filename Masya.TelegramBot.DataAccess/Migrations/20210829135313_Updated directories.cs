using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Updateddirectories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_References_Agencies_AgencyId",
                table: "References");

            migrationBuilder.DropIndex(
                name: "IX_References_AgencyId",
                table: "References");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "References");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "References",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "DirectoryItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "References");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "DirectoryItems");

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "References",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_References_AgencyId",
                table: "References",
                column: "AgencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_References_Agencies_AgencyId",
                table: "References",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
