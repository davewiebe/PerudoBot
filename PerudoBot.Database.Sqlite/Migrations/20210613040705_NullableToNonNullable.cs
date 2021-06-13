using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class NullableToNonNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerTurnId",
                table: "Games");

            migrationBuilder.AddColumn<int>(
                name: "GamePlayerTurnId",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GamePlayerTurnId",
                table: "Games");

            migrationBuilder.AddColumn<int>(
                name: "PlayerTurnId",
                table: "Games",
                type: "INTEGER",
                nullable: true);
        }
    }
}
