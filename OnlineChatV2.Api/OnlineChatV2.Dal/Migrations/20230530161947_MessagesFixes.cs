using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineChatV2.Dal.Migrations
{
    public partial class MessagesFixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ToUserId",
                table: "Messages");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ToUserId",
                table: "Messages",
                newName: "IX_PrivateChats");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_FromUserId",
                table: "Messages",
                newName: "IX_Sender");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                newName: "IX_Chats");

            migrationBuilder.AlterColumn<long>(
                name: "ToUserId",
                table: "Messages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "Messages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "MessageDate",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Chats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_OwnerId",
                table: "Chats",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Users_OwnerId",
                table: "Chats",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ToUserId",
                table: "Messages",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Users_OwnerId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ToUserId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Chats_OwnerId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "MessageDate",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Chats");

            migrationBuilder.RenameIndex(
                name: "IX_Sender",
                table: "Messages",
                newName: "IX_Messages_FromUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PrivateChats",
                table: "Messages",
                newName: "IX_Messages_ToUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Chats",
                table: "Messages",
                newName: "IX_Messages_ChatId");

            migrationBuilder.AlterColumn<long>(
                name: "ToUserId",
                table: "Messages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ChatId",
                table: "Messages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatId",
                table: "Messages",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ToUserId",
                table: "Messages",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
