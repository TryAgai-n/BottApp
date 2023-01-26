using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BottApp.Host.Migrations
{
    public partial class UserFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFlag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    IsSendCaption = table.Column<bool>(type: "boolean", nullable: false),
                    IsSendDocument = table.Column<bool>(type: "boolean", nullable: false),
                    IsSendNomination = table.Column<bool>(type: "boolean", nullable: false),
                    IsSendPhone = table.Column<bool>(type: "boolean", nullable: false),
                    IsSendLastName = table.Column<bool>(type: "boolean", nullable: false),
                    IsSendFirstName = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlag_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFlag_UserId",
                table: "UserFlag",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFlag");
        }
    }
}
