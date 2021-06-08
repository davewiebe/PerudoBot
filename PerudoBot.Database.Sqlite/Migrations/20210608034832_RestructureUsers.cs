using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class RestructureUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Players",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "GamePlayerRoundId",
                table: "Actions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions",
                column: "GamePlayerRoundId",
                principalTable: "GamePlayerRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Players",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Players",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GamePlayerRoundId",
                table: "Actions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions",
                column: "GamePlayerRoundId",
                principalTable: "GamePlayerRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
