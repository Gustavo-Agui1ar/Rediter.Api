using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USER_BLOCKS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    BLOCKER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    BLOCKED_ID = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_BLOCKS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_USER_BLOCKS_USERS_BLOCKED_ID",
                        column: x => x.BLOCKED_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USER_BLOCKS_USERS_BLOCKER_ID",
                        column: x => x.BLOCKER_ID,
                        principalTable: "USERS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USER_BLOCKS_BLOCKED_ID",
                table: "USER_BLOCKS",
                column: "BLOCKED_ID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_BLOCKS_BLOCKER_ID_BLOCKED_ID",
                table: "USER_BLOCKS",
                columns: new[] { "BLOCKER_ID", "BLOCKED_ID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USER_BLOCKS");
        }
    }
}
