using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineChatV2.Dal.Migrations
{
    public partial class Contacts3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserContact_Users_ContactId",
                table: "UserContact");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContact_Users_ContactOwnerId",
                table: "UserContact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserContact",
                table: "UserContact");

            migrationBuilder.RenameTable(
                name: "UserContact",
                newName: "UsersContacts");

            migrationBuilder.RenameIndex(
                name: "IX_UserContact_ContactOwnerId",
                table: "UsersContacts",
                newName: "IX_UsersContacts_ContactOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_UserContact_ContactId",
                table: "UsersContacts",
                newName: "IX_UsersContacts_ContactId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersContacts",
                table: "UsersContacts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersContacts_Users_ContactId",
                table: "UsersContacts",
                column: "ContactId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersContacts_Users_ContactOwnerId",
                table: "UsersContacts",
                column: "ContactOwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersContacts_Users_ContactId",
                table: "UsersContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersContacts_Users_ContactOwnerId",
                table: "UsersContacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersContacts",
                table: "UsersContacts");

            migrationBuilder.RenameTable(
                name: "UsersContacts",
                newName: "UserContact");

            migrationBuilder.RenameIndex(
                name: "IX_UsersContacts_ContactOwnerId",
                table: "UserContact",
                newName: "IX_UserContact_ContactOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_UsersContacts_ContactId",
                table: "UserContact",
                newName: "IX_UserContact_ContactId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserContact",
                table: "UserContact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserContact_Users_ContactId",
                table: "UserContact",
                column: "ContactId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContact_Users_ContactOwnerId",
                table: "UserContact",
                column: "ContactOwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
