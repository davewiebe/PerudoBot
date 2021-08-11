using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class Metadata4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Metadata",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metadata_GameId",
                table: "Metadata",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Metadata_Games_GameId",
                table: "Metadata",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Metadata_Games_GameId",
                table: "Metadata");

            migrationBuilder.DropIndex(
                name: "IX_Metadata_GameId",
                table: "Metadata");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Metadata");
        }
    }
}
