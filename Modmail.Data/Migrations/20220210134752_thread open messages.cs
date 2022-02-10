using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modmail.Data.Migrations
{
    public partial class threadopenmessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Closed",
                table: "Threads",
                newName: "Open");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "Threads",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModThreadOpenMessage",
                table: "Configs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserThreadOpenMessage",
                table: "Configs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "ModThreadOpenMessage",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "UserThreadOpenMessage",
                table: "Configs");

            migrationBuilder.RenameColumn(
                name: "Open",
                table: "Threads",
                newName: "Closed");
        }
    }
}
