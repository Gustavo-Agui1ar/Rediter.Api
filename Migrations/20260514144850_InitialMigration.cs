using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PICTURES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FILE_NAME = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    STORAGE_PATH = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    MIME_TYPE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    FILE_SIZE_BYTES = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PICTURES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PASSWORD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PROFILE_PICTURE_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    PROFILE_COVER_ID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    IS_VERIFIED = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: false),
                    VERIFICATION_CODE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    REFRESH_TOKEN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    REFRESH_TOKEN_EXPIRATION = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USERS_PICTURES_PROFILE_COVER_ID",
                        column: x => x.PROFILE_COVER_ID,
                        principalTable: "PICTURES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_USERS_PICTURES_PROFILE_PICTURE_ID",
                        column: x => x.PROFILE_PICTURE_ID,
                        principalTable: "PICTURES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "POSTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USERID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PARENTPOSTID = table.Column<Guid>(type: "RAW(16)", nullable: true),
                    COMMENTSCOUNT = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LOCATIONNAME = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: true),
                    CONTENT = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    CREATEDAT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UPDATEDAT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LIKESCOUNT = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_POSTS_POSTS_PARENTPOSTID",
                        column: x => x.PARENTPOSTID,
                        principalTable: "POSTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POSTS_USERS_USERID",
                        column: x => x.USERID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_FOLLOWERS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FOLLOWER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FOLLOWING_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "POST_IMAGES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    POST_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PICTURE_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POST_IMAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_POST_IMAGES_PICTURES_PICTURE_ID",
                        column: x => x.PICTURE_ID,
                        principalTable: "PICTURES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_POST_IMAGES_POSTS_POST_ID",
                        column: x => x.POST_ID,
                        principalTable: "POSTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_POST_LIKES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    POST_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_POST_LIKES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USER_POST_LIKES_POSTS_POST_ID",
                        column: x => x.POST_ID,
                        principalTable: "POSTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_POST_LIKES_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_POST_IMAGES_PICTURE_ID",
                table: "POST_IMAGES",
                column: "PICTURE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_POST_IMAGES_POST_ID",
                table: "POST_IMAGES",
                column: "POST_ID");

            migrationBuilder.CreateIndex(
                name: "IX_POSTS_PARENTPOSTID",
                table: "POSTS",
                column: "PARENTPOSTID");

            migrationBuilder.CreateIndex(
                name: "IX_POSTS_USERID",
                table: "POSTS",
                column: "USERID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWER_ID_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                columns: new[] { "FOLLOWER_ID", "FOLLOWING_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_FOLLOWERS_FOLLOWING_ID",
                table: "USER_FOLLOWERS",
                column: "FOLLOWING_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_POST_LIKES_POST_ID",
                table: "USER_POST_LIKES",
                column: "POST_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_POST_LIKES_USER_ID_POST_ID",
                table: "USER_POST_LIKES",
                columns: new[] { "USER_ID", "POST_ID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_PROFILE_COVER_ID",
                table: "USERS",
                column: "PROFILE_COVER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_PROFILE_PICTURE_ID",
                table: "USERS",
                column: "PROFILE_PICTURE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "POST_IMAGES");

            migrationBuilder.DropTable(
                name: "USER_FOLLOWERS");

            migrationBuilder.DropTable(
                name: "USER_POST_LIKES");

            migrationBuilder.DropTable(
                name: "POSTS");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "PICTURES");
        }
    }
}
