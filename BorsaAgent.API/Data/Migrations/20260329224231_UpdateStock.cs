using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorsaAgent.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stock_ShortCode",
                table: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ShortCode",
                table: "Stock",
                column: "ShortCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stock_ShortCode",
                table: "Stock");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ShortCode",
                table: "Stock",
                column: "ShortCode");
        }
    }
}
