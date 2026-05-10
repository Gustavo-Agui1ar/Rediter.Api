using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POST_IMAGES_PICTURES_PICTURE_ID",
                table: "POST_IMAGES");

            migrationBuilder.DropForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_COVER_ID",
                table: "USERS");

            migrationBuilder.DropForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_PICTURE_ID",
                table: "USERS");

            migrationBuilder.AlterColumn<string>(
                name: "NAME",
                table: "USERS",
                type: "NVARCHAR2(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_VERIFIED",
                table: "USERS",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");

            migrationBuilder.AlterColumn<string>(
                name: "EMAIL",
                table: "USERS",
                type: "NVARCHAR2(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.AlterColumn<string>(
                name: "DESCRIPTION",
                table: "USERS",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LOCATION_NAME",
                table: "POSTS",
                type: "NVARCHAR2(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CONTENT",
                table: "POSTS",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DISPLAY_ORDER",
                table: "POST_IMAGES",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)");

            migrationBuilder.AlterColumn<string>(
                name: "STORAGE_PATH",
                table: "PICTURES",
                type: "NVARCHAR2(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.AlterColumn<string>(
                name: "MIME_TYPE",
                table: "PICTURES",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.AlterColumn<string>(
                name: "FILE_NAME",
                table: "PICTURES",
                type: "NVARCHAR2(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(2000)");

            migrationBuilder.CreateTable(
                name: "USER_FOLLOWERS",
                columns: table => new
                {
                    FOLLOWER = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOLLOWING = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_FOLLOWERS", x => new { x.FOLLOWER, x.FOLLOWING });
                    table.ForeignKey(
                        name: "FK_USER_FOLLOWERS_USERS_FOLLOWER",
                        column: x => x.FOLLOWER,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USER_FOLLOWERS_USERS_FOLLOWING",
                        column: x => x.FOLLOWING,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWING",
                table: "USER_FOLLOWERS",
                column: "FOLLOWING");

            migrationBuilder.AddForeignKey(
                name: "FK_POST_IMAGES_PICTURES_PICTURE_ID",
                table: "POST_IMAGES",
                column: "PICTURE_ID",
                principalTable: "PICTURES",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_COVER_ID",
                table: "USERS",
                column: "PROFILE_COVER_ID",
                principalTable: "PICTURES",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_PICTURE_ID",
                table: "USERS",
                column: "PROFILE_PICTURE_ID",
                principalTable: "PICTURES",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POST_IMAGES_PICTURES_PICTURE_ID",
                table: "POST_IMAGES");

            migrationBuilder.DropForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_COVER_ID",
                table: "USERS");

            migrationBuilder.DropForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_PICTURE_ID",
                table: "USERS");

            migrationBuilder.DropTable(
                name: "USER_FOLLOWERS");

            migrationBuilder.AlterColumn<string>(
                name: "NAME",
                table: "USERS",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IS_VERIFIED",
                table: "USERS",
                type: "NUMBER(1)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "EMAIL",
                table: "USERS",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "DESCRIPTION",
                table: "USERS",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LOCATION_NAME",
                table: "POSTS",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CONTENT",
                table: "POSTS",
                type: "NVARCHAR2(2000)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DISPLAY_ORDER",
                table: "POST_IMAGES",
                type: "NUMBER(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "NUMBER(10)",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "STORAGE_PATH",
                table: "PICTURES",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "MIME_TYPE",
                table: "PICTURES",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FILE_NAME",
                table: "PICTURES",
                type: "NVARCHAR2(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "NVARCHAR2(255)",
                oldMaxLength: 255);

            migrationBuilder.AddForeignKey(
                name: "FK_POST_IMAGES_PICTURES_PICTURE_ID",
                table: "POST_IMAGES",
                column: "PICTURE_ID",
                principalTable: "PICTURES",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_COVER_ID",
                table: "USERS",
                column: "PROFILE_COVER_ID",
                principalTable: "PICTURES",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_USERS_PICTURES_PROFILE_PICTURE_ID",
                table: "USERS",
                column: "PROFILE_PICTURE_ID",
                principalTable: "PICTURES",
                principalColumn: "ID");
        }
    }
}
