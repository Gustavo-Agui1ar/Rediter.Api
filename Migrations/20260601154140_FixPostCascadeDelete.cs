using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixPostCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POSTS_POSTS_PARENTPOSTID",
                table: "POSTS");

            migrationBuilder.AddForeignKey(
                name: "FK_POSTS_POSTS_PARENTPOSTID",
                table: "POSTS",
                column: "PARENTPOSTID",
                principalTable: "POSTS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POSTS_POSTS_PARENTPOSTID",
                table: "POSTS");

            migrationBuilder.AddForeignKey(
                name: "FK_POSTS_POSTS_PARENTPOSTID",
                table: "POSTS",
                column: "PARENTPOSTID",
                principalTable: "POSTS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
