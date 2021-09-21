using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Updatedagentidtrackingfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RealtyObjects_Users_AgentId1",
                table: "RealtyObjects");

            migrationBuilder.DropIndex(
                name: "IX_RealtyObjects_AgentId1",
                table: "RealtyObjects");

            migrationBuilder.DropColumn(
                name: "AgentId1",
                table: "RealtyObjects");

            migrationBuilder.AlterColumn<long>(
                name: "AgentId",
                table: "RealtyObjects",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_AgentId",
                table: "RealtyObjects",
                column: "AgentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RealtyObjects_Users_AgentId",
                table: "RealtyObjects",
                column: "AgentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RealtyObjects_Users_AgentId",
                table: "RealtyObjects");

            migrationBuilder.DropIndex(
                name: "IX_RealtyObjects_AgentId",
                table: "RealtyObjects");

            migrationBuilder.AlterColumn<int>(
                name: "AgentId",
                table: "RealtyObjects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AgentId1",
                table: "RealtyObjects",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RealtyObjects_AgentId1",
                table: "RealtyObjects",
                column: "AgentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RealtyObjects_Users_AgentId1",
                table: "RealtyObjects",
                column: "AgentId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
