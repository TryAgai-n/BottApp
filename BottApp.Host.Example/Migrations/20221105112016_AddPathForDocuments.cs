using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Example.Migrations
{
    public partial class AddPathForDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DocumentModel",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "DocumentModel");
        }
    }
}
