using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedChatColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USERFOLLOWERS");

            migrationBuilder.AddColumn<DateTime>(
                name: "UPDATE_AT",
                table: "CHATS",
                type: "TIMESTAMP(7)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateTable(
                name: "USER_FOLLOWERS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOLLOWER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOLLOWING_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_FOLLOWERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USER_FOLLOWERS_USERS_FOLLOWER_ID",
                        column: x => x.FOLLOWER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USER_FOLLOWERS_USERS_FOLLOWING_ID",
                        column: x => x.FOLLOWING_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWER_ID_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                columns: new[] { "FOLLOWER_ID", "FOLLOWING_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                column: "FOLLOWING_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USER_FOLLOWERS");

            migrationBuilder.DropColumn(
                name: "UPDATE_AT",
                table: "CHATS");

            migrationBuilder.CreateTable(
                name: "USERFOLLOWERS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOLLOWERID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOLLOWINGID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CREATEDAT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERFOLLOWERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USERFOLLOWERS_USERS_FOLLOWERID",
                        column: x => x.FOLLOWERID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USERFOLLOWERS_USERS_FOLLOWINGID",
                        column: x => x.FOLLOWINGID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USERFOLLOWERS_FOLLOWERID",
                table: "USERFOLLOWERS",
                column: "FOLLOWERID");

            migrationBuilder.CreateIndex(
                name: "IX_USERFOLLOWERS_FOLLOWINGID",
                table: "USERFOLLOWERS",
                column: "FOLLOWINGID");
        }
    }
}
