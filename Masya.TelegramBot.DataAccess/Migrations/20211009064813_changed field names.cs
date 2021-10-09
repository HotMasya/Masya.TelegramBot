using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class changedfieldnames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_RealtyObjects_PropertyObjectId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_PropertyObjectId",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "PropertyObjectId",
                table: "Favorites");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Favorites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RealtyObjectId",
                table: "Favorites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_RealtyObjectId",
                table: "Favorites",
                column: "RealtyObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_RealtyObjects_RealtyObjectId",
                table: "Favorites",
                column: "RealtyObjectId",
                principalTable: "RealtyObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_RealtyObjects_RealtyObjectId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_RealtyObjectId",
                table: "Favorites");

            migrationBuilder.DropColumn(
                name: "RealtyObjectId",
                table: "Favorites");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Favorites",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PropertyObjectId",
                table: "Favorites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_PropertyObjectId",
                table: "Favorites",
                column: "PropertyObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_RealtyObjects_PropertyObjectId",
                table: "Favorites",
                column: "PropertyObjectId",
                principalTable: "RealtyObjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
