using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class AddBetActionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayers_GamePlayerId",
                table: "Actions");

            migrationBuilder.AlterColumn<int>(
                name: "GamePlayerRoundId",
                table: "Actions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "GamePlayerId",
                table: "Actions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "BetAmount",
                table: "Actions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BetType",
                table: "Actions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BettingPlayerId",
                table: "Actions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetActionId",
                table: "Actions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_BettingPlayerId",
                table: "Actions",
                column: "BettingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Actions_TargetActionId",
                table: "Actions",
                column: "TargetActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Actions_TargetActionId",
                table: "Actions",
                column: "TargetActionId",
                principalTable: "Actions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions",
                column: "GamePlayerRoundId",
                principalTable: "GamePlayerRounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayers_GamePlayerId",
                table: "Actions",
                column: "GamePlayerId",
                principalTable: "GamePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Players_BettingPlayerId",
                table: "Actions",
                column: "BettingPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Actions_TargetActionId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayerRounds_GamePlayerRoundId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_GamePlayers_GamePlayerId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Players_BettingPlayerId",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_BettingPlayerId",
                table: "Actions");

            migrationBuilder.DropIndex(
                name: "IX_Actions_TargetActionId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "BetAmount",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "BetType",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "BettingPlayerId",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "TargetActionId",
                table: "Actions");

            migrationBuilder.AlterColumn<int>(
                name: "GamePlayerRoundId",
                table: "Actions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GamePlayerId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_GamePlayers_GamePlayerId",
                table: "Actions",
                column: "GamePlayerId",
                principalTable: "GamePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
