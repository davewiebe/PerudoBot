using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class MoreRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoundStartPlayerId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Dice",
                table: "GamePlayers");

            migrationBuilder.DropColumn(
                name: "IsEliminated",
                table: "GamePlayerRounds");

            migrationBuilder.DropColumn(
                name: "NumberOfDice",
                table: "GamePlayerRounds");

            migrationBuilder.DropColumn(
                name: "TurnOrder",
                table: "GamePlayerRounds");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoundStartPlayerId",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Dice",
                table: "GamePlayers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEliminated",
                table: "GamePlayerRounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDice",
                table: "GamePlayerRounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TurnOrder",
                table: "GamePlayerRounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
