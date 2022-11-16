using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class addStatesForUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserState",
                table: "User",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserState",
                table: "User");
        }
    }
}
