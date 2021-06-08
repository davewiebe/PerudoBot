using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class RestructuringData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFinished",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DateStarted",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "Rounds");

            migrationBuilder.DropColumn(
                name: "CanCallExactToJoinAgain",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "DateFinished",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "DateStarted",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Winner",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "DurationInSeconds",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "IsAutoAction",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "IsOutOfTurn",
                table: "Actions");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "Actions");

            migrationBuilder.RenameColumn(
                name: "StatusMessage",
                table: "Games",
                newName: "WinnerPlayerId");

            migrationBuilder.AddColumn<bool>(
                name: "IsEliminated",
                table: "GamePlayerRounds",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEliminated",
                table: "GamePlayerRounds");

            migrationBuilder.RenameColumn(
                name: "WinnerPlayerId",
                table: "Games",
                newName: "StatusMessage");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFinished",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStarted",
                table: "Rounds",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "DurationInSeconds",
                table: "Rounds",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanCallExactToJoinAgain",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFinished",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStarted",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Winner",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DurationInSeconds",
                table: "Actions",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoAction",
                table: "Actions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutOfTurn",
                table: "Actions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "Actions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
