using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolLogix.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedByUserToDetailUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsedByUserId",
                table: "DetailUsages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedByUserId",
                table: "DetailUsages");
        }
    }
}
