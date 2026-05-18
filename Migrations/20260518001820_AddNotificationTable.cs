using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NOTIFICATIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RECIPIENT_USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    SENDER_USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    POST_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TYPE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IS_READ = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NOTIFICATIONS_POSTS_POST_ID",
                        column: x => x.POST_ID,
                        principalTable: "POSTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NOTIFICATIONS_USERS_RECIPIENT_USER_ID",
                        column: x => x.RECIPIENT_USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NOTIFICATIONS_USERS_SENDER_USER_ID",
                        column: x => x.SENDER_USER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_CREATED_AT",
                table: "NOTIFICATIONS",
                column: "CREATED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_POST_ID",
                table: "NOTIFICATIONS",
                column: "POST_ID");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_RECIPIENT_USER_ID",
                table: "NOTIFICATIONS",
                column: "RECIPIENT_USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_RECIPIENT_USER_ID_IS_READ",
                table: "NOTIFICATIONS",
                columns: new[] { "RECIPIENT_USER_ID", "IS_READ" });

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATIONS_SENDER_USER_ID",
                table: "NOTIFICATIONS",
                column: "SENDER_USER_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NOTIFICATIONS");
        }
    }
}
