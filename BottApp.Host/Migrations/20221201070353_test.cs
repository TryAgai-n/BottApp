using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Nomination",
                table: "User",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nomination",
                table: "User");
        }
    }
}
