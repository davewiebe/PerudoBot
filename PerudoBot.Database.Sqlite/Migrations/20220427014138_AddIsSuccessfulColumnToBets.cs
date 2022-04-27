using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class AddIsSuccessfulColumnToBets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuccessful",
                table: "Actions",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuccessful",
                table: "Actions");
        }
    }
}
