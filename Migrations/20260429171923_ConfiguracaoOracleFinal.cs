using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracaoOracleFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PICTURES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FILE_NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    STORAGE_PATH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MIME_TYPE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
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
                    NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PASSWORD = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PROFILE_PICTURE_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PROFILE_COVER_ID = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    IS_VERIFIED = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    VERIFICATION_CODE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    REFRESH_TOKEN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    REFRESH_TOKEN_EXPIRATION = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USERS_PICTURES_PROFILE_COVER_ID",
                        column: x => x.PROFILE_COVER_ID,
                        principalTable: "PICTURES",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_USERS_PICTURES_PROFILE_PICTURE_ID",
                        column: x => x.PROFILE_PICTURE_ID,
                        principalTable: "PICTURES",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "POSTS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    LOCATION_NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CONTENT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_POSTS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POST_IMAGES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    POST_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    PICTURE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DISPLAY_ORDER = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POST_IMAGES", x => x.ID);
                    table.ForeignKey(
                        name: "FK_POST_IMAGES_PICTURES_PICTURE_ID",
                        column: x => x.PICTURE_ID,
                        principalTable: "PICTURES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_POST_IMAGES_POSTS_POST_ID",
                        column: x => x.POST_ID,
                        principalTable: "POSTS",
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
                name: "IX_POSTS_USER_ID",
                table: "POSTS",
                column: "USER_ID");

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
                name: "POSTS");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "PICTURES");
        }
    }
}
