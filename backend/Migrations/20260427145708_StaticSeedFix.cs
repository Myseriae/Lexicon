using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lexicon.Migrations
{
    /// <inheritdoc />
    public partial class StaticSeedFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Articles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Created",
                value: new DateTime(2026, 4, 27, 15, 43, 24, 467, DateTimeKind.Local).AddTicks(2560));

            migrationBuilder.InsertData(
                table: "Articles",
                columns: new[] { "Id", "Content", "Created", "Title" },
                values: new object[,]
                {
                    { 2, "You use it to build a web application.", new DateTime(2026, 4, 27, 15, 43, 24, 480, DateTimeKind.Local).AddTicks(6620), "ASP.NET Core" },
                    { 3, "Used to build RESTful APIs.", new DateTime(2026, 4, 27, 15, 43, 24, 480, DateTimeKind.Local).AddTicks(6630), "REST API" }
                });
        }
    }
}
