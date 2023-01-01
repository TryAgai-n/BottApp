using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class ViewMessageIdForUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ViewDocumentID",
                table: "User",
                newName: "ViewDocumentId");

            migrationBuilder.AddColumn<int>(
                name: "ViewMessageId",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewMessageId",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "ViewDocumentId",
                table: "User",
                newName: "ViewDocumentID");
        }
    }
}
