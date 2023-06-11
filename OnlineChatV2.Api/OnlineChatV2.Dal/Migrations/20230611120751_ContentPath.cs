using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineChatV2.Dal.Migrations
{
    public partial class ContentPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentPath",
                table: "MessageContent",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentPath",
                table: "MessageContent");
        }
    }
}
