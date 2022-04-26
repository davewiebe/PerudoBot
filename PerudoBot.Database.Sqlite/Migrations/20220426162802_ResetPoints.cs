using Microsoft.EntityFrameworkCore.Migrations;

namespace PerudoBot.Database.Sqlite.Migrations
{
    public partial class ResetPoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Players] SET [UsedPoints] = 0");
            migrationBuilder.Sql("UPDATE [Players] SET [TotalPoints] = 200");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
