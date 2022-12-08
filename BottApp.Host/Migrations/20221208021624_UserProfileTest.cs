using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class UserProfileTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "Nomination",
                table: "User",
                newName: "InNomination");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InNomination",
                table: "User",
                newName: "Nomination");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "User",
                type: "text",
                nullable: true);
        }
    }
}
