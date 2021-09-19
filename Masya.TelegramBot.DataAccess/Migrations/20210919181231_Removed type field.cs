using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Removedtypefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RealtyObjects_DirectoryItems_TypeId",
                table: "RealtyObjects");

            migrationBuilder.DropIndex(
                name: "IX_RealtyObjects_TypeId",
                table: "RealtyObjects");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "RealtyObjects");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "RealtyObjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_TypeId",
                table: "RealtyObjects",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RealtyObjects_DirectoryItems_TypeId",
                table: "RealtyObjects",
                column: "TypeId",
                principalTable: "DirectoryItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
