using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorsaAgent.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStockSyncLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockSyncLog_StockId_Period1Utc_Period2Utc_Interval",
                table: "StockSyncLog");

            migrationBuilder.DropColumn(
                name: "InsertedCount",
                table: "StockSyncLog");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "StockSyncLog");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StockSyncLog");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "StockSyncLog",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestUrl",
                table: "StockSyncLog",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StockSyncLog_CreatedAtUtc",
                table: "StockSyncLog",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StockSyncLog_StockId",
                table: "StockSyncLog",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockSyncLog_CreatedAtUtc",
                table: "StockSyncLog");

            migrationBuilder.DropIndex(
                name: "IX_StockSyncLog_StockId",
                table: "StockSyncLog");

            migrationBuilder.DropColumn(
                name: "RequestUrl",
                table: "StockSyncLog");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "StockSyncLog",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "InsertedCount",
                table: "StockSyncLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Interval",
                table: "StockSyncLog",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "StockSyncLog",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StockSyncLog_StockId_Period1Utc_Period2Utc_Interval",
                table: "StockSyncLog",
                columns: new[] { "StockId", "Period1Utc", "Period2Utc", "Interval" });
        }
    }
}
