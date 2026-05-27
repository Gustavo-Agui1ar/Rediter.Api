using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWER_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USER_FOLLOWERS",
                table: "USER_FOLLOWERS");

            migrationBuilder.DropIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWER_ID_FOLLOWING_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.RenameTable(
                name: "USER_FOLLOWERS",
                newName: "USERFOLLOWERS");

            migrationBuilder.RenameColumn(
                name: "FOLLOWING_ID",
                table: "USERFOLLOWERS",
                newName: "FOLLOWINGID");

            migrationBuilder.RenameColumn(
                name: "FOLLOWER_ID",
                table: "USERFOLLOWERS",
                newName: "FOLLOWERID");

            migrationBuilder.RenameColumn(
                name: "CREATED_AT",
                table: "USERFOLLOWERS",
                newName: "CREATEDAT");

            migrationBuilder.RenameIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWING_ID",
                table: "USERFOLLOWERS",
                newName: "IX_USERFOLLOWERS_FOLLOWINGID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CREATEDAT",
                table: "USERFOLLOWERS",
                type: "TIMESTAMP(7)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USERFOLLOWERS",
                table: "USERFOLLOWERS",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "CHATS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TITLE = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    IS_GROUP = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHATS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CHAT_PARTICIPANTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CHAT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    IS_MUTED = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHAT_PARTICIPANTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CHAT_PARTICIPANTS_CHATS_CHAT_ID",
                        column: x => x.CHAT_ID,
                        principalTable: "CHATS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CHAT_PARTICIPANTS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MESSAGES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CHAT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SENDER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CONTENT = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    IS_DELETED = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MESSAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MESSAGES_CHATS_CHAT_ID",
                        column: x => x.CHAT_ID,
                        principalTable: "CHATS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MESSAGES_USERS_SENDER_ID",
                        column: x => x.SENDER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USERFOLLOWERS_FOLLOWERID",
                table: "USERFOLLOWERS",
                column: "FOLLOWERID");

            migrationBuilder.CreateIndex(
                name: "idx_chat_participant_unique",
                table: "CHAT_PARTICIPANTS",
                columns: new[] { "CHAT_ID", "USER_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_PARTICIPANTS_USER_ID",
                table: "CHAT_PARTICIPANTS",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "idx_message_chat_id",
                table: "MESSAGES",
                column: "CHAT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MESSAGES_SENDER_ID",
                table: "MESSAGES",
                column: "SENDER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_USERFOLLOWERS_USERS_FOLLOWERID",
                table: "USERFOLLOWERS",
                column: "FOLLOWERID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USERFOLLOWERS_USERS_FOLLOWINGID",
                table: "USERFOLLOWERS",
                column: "FOLLOWINGID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USERFOLLOWERS_USERS_FOLLOWERID",
                table: "USERFOLLOWERS");

            migrationBuilder.DropForeignKey(
                name: "FK_USERFOLLOWERS_USERS_FOLLOWINGID",
                table: "USERFOLLOWERS");

            migrationBuilder.DropTable(
                name: "CHAT_PARTICIPANTS");

            migrationBuilder.DropTable(
                name: "MESSAGES");

            migrationBuilder.DropTable(
                name: "CHATS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USERFOLLOWERS",
                table: "USERFOLLOWERS");

            migrationBuilder.DropIndex(
                name: "IX_USERFOLLOWERS_FOLLOWERID",
                table: "USERFOLLOWERS");

            migrationBuilder.RenameTable(
                name: "USERFOLLOWERS",
                newName: "USER_FOLLOWERS");

            migrationBuilder.RenameColumn(
                name: "FOLLOWINGID",
                table: "USER_FOLLOWERS",
                newName: "FOLLOWING_ID");

            migrationBuilder.RenameColumn(
                name: "FOLLOWERID",
                table: "USER_FOLLOWERS",
                newName: "FOLLOWER_ID");

            migrationBuilder.RenameColumn(
                name: "CREATEDAT",
                table: "USER_FOLLOWERS",
                newName: "CREATED_AT");

            migrationBuilder.RenameIndex(
                name: "IX_USERFOLLOWERS_FOLLOWINGID",
                table: "USER_FOLLOWERS",
                newName: "IX_USER_FOLLOWERS_FOLLOWING_ID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CREATED_AT",
                table: "USER_FOLLOWERS",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP(7)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USER_FOLLOWERS",
                table: "USER_FOLLOWERS",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWER_ID_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                columns: new[] { "FOLLOWER_ID", "FOLLOWING_ID" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWER_ID",
                table: "USER_FOLLOWERS",
                column: "FOLLOWER_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                column: "FOLLOWING_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
