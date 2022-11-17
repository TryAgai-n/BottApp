using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class addUserOnState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OnState",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE \"User\" SET \"OnState\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnState",
                table: "User");
        }
    }
}
