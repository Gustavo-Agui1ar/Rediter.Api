using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteToUserRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKED_ID",
                table: "USER_BLOCKS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKER_ID",
                table: "USER_BLOCKS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWER_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.AddForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKED_ID",
                table: "USER_BLOCKS",
                column: "BLOCKED_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKER_ID",
                table: "USER_BLOCKS",
                column: "BLOCKER_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWER_ID",
                table: "USER_FOLLOWERS",
                column: "FOLLOWER_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                column: "FOLLOWING_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKED_ID",
                table: "USER_BLOCKS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKER_ID",
                table: "USER_BLOCKS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWER_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.DropForeignKey(
                name: "FK_USER_FOLLOWERS_USERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS");

            migrationBuilder.AddForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKED_ID",
                table: "USER_BLOCKS",
                column: "BLOCKED_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_BLOCKS_USERS_BLOCKER_ID",
                table: "USER_BLOCKS",
                column: "BLOCKER_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

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
