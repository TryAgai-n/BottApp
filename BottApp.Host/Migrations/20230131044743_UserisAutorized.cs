using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class UserisAutorized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isAuthorized",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isAuthorized",
                table: "User");
        }
    }
}
