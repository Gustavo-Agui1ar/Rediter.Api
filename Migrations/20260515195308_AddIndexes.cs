using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CONTENT",
                table: "POSTS",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NAMELOWER",
                table: "USERS",
                type: "NVARCHAR2(450)",
                nullable: true,
                computedColumnSql: "LOWER(\"NAME\")");

            migrationBuilder.AddColumn<string>(
                name: "CONTENTLOWER",
                table: "POSTS",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true,
                computedColumnSql: "LOWER(\"CONTENT\")");

            migrationBuilder.CreateIndex(
                name: "idx_user_name_lower",
                table: "USERS",
                column: "NAMELOWER");

            migrationBuilder.CreateIndex(
                name: "idx_post_content_lower",
                table: "POSTS",
                column: "CONTENTLOWER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_user_name_lower",
                table: "USERS");

            migrationBuilder.DropIndex(
                name: "idx_post_content_lower",
                table: "POSTS");

            migrationBuilder.DropColumn(
                name: "NAMELOWER",
                table: "USERS");

            migrationBuilder.DropColumn(
                name: "CONTENTLOWER",
                table: "POSTS");

            migrationBuilder.AlterColumn<string>(
                name: "CONTENT",
                table: "POSTS",
                type: "NVARCHAR2(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
