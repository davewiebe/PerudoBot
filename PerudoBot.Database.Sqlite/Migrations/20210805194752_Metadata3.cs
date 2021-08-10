using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class Metadata3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GamePlayerId",
                table: "Metadata",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_GamePlayerId",
                table: "Metadata",
                column: "GamePlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Metadata_GamePlayers_GamePlayerId",
                table: "Metadata",
                column: "GamePlayerId",
                principalTable: "GamePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Metadata_GamePlayers_GamePlayerId",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_GamePlayerId",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "GamePlayerId",
                table: "Metadata");
        }
    }
}
