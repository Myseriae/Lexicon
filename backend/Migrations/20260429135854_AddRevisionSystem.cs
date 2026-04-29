using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lexicon.Migrations
{
    /// <inheritdoc />
    public partial class AddRevisionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Revisions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Revisions");
        }
    }
}
