using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BorsaAgent.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateMlModelMetric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MLModelMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RSquared = table.Column<double>(type: "double precision", nullable: false),
                    Rmse = table.Column<double>(type: "double precision", nullable: false),
                    Mae = table.Column<double>(type: "double precision", nullable: false),
                    TrainingDataCount = table.Column<int>(type: "integer", nullable: false),
                    ModelPath = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModelMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MLModelMetrics_CreatedAtUtc",
                table: "MLModelMetrics",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MLModelMetrics");
        }
    }
}
