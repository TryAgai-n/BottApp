using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Example.Migrations
{
    public partial class AddTypeForMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Message",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Message");
        }
    }
}
