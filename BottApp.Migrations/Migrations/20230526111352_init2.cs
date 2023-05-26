using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BottApp.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Document",
                newName: "PathHalfQuality");

            migrationBuilder.AddColumn<string>(
                name: "PathFullQuality",
                table: "Document",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathFullQuality",
                table: "Document");

            migrationBuilder.RenameColumn(
                name: "PathHalfQuality",
                table: "Document",
                newName: "Path");
        }
    }
}
