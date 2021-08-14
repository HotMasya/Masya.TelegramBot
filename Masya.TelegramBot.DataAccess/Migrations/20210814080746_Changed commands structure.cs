using Microsoft.EntityFrameworkCore.Migrations;

namespace Masya.TelegramBot.DataAccess.Migrations
{
    public partial class Changedcommandsstructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aliases");

            migrationBuilder.AddColumn<int>(
                name: "CommandId",
                table: "Commands",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DisplayInMenu",
                table: "Commands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Commands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Commands_CommandId",
                table: "Commands",
                column: "CommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Commands_Commands_CommandId",
                table: "Commands",
                column: "CommandId",
                principalTable: "Commands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commands_Commands_CommandId",
                table: "Commands");

            migrationBuilder.DropIndex(
                name: "IX_Commands_CommandId",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "CommandId",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "DisplayInMenu",
                table: "Commands");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Commands");

            migrationBuilder.CreateTable(
                name: "Aliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommandId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aliases_Commands_CommandId",
                        column: x => x.CommandId,
                        principalTable: "Commands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aliases_CommandId",
                table: "Aliases",
                column: "CommandId");
        }
    }
}
