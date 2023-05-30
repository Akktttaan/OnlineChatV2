using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineChatV2.Dal.Migrations
{
    public partial class IX_Username : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Username",
                table: "Users",
                column: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Username",
                table: "Users");
        }
    }
}
