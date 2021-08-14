using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class AddedParentIdtocommandmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commands_Commands_CommandId",
                table: "Commands");

            migrationBuilder.RenameColumn(
                name: "CommandId",
                table: "Commands",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Commands_CommandId",
                table: "Commands",
                newName: "IX_Commands_ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commands_Commands_ParentId",
                table: "Commands",
                column: "ParentId",
                principalTable: "Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commands_Commands_ParentId",
                table: "Commands");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "Commands",
                newName: "CommandId");

            migrationBuilder.RenameIndex(
                name: "IX_Commands_ParentId",
                table: "Commands",
                newName: "IX_Commands_CommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commands_Commands_CommandId",
                table: "Commands",
                column: "CommandId",
                principalTable: "Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
