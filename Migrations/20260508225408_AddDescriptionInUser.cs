using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rediter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DESCRIPTION",
                table: "USERS",
                type: "NVARCHAR2(2000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DESCRIPTION",
                table: "USERS");
        }
    }
}
