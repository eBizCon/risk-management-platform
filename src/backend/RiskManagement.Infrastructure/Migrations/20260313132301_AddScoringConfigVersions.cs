using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RiskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringConfigVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "scoring_config_version_id",
                table: "applications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "scoring_config_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version = table.Column<int>(type: "integer", nullable: false),
                    green_threshold = table.Column<int>(type: "integer", nullable: false),
                    yellow_threshold = table.Column<int>(type: "integer", nullable: false),
                    income_ratio_good = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    income_ratio_moderate = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    income_ratio_limited = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    penalty_moderate_ratio = table.Column<int>(type: "integer", nullable: false),
                    penalty_limited_ratio = table.Column<int>(type: "integer", nullable: false),
                    penalty_critical_ratio = table.Column<int>(type: "integer", nullable: false),
                    rate_good = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    rate_moderate = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    rate_heavy = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    penalty_moderate_rate = table.Column<int>(type: "integer", nullable: false),
                    penalty_heavy_rate = table.Column<int>(type: "integer", nullable: false),
                    penalty_excessive_rate = table.Column<int>(type: "integer", nullable: false),
                    penalty_self_employed = table.Column<int>(type: "integer", nullable: false),
                    penalty_retired = table.Column<int>(type: "integer", nullable: false),
                    penalty_unemployed = table.Column<int>(type: "integer", nullable: false),
                    penalty_payment_default = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scoring_config_versions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_applications_scoring_config_version_id",
                table: "applications",
                column: "scoring_config_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_scoring_config_versions_version",
                table: "scoring_config_versions",
                column: "version",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_applications_scoring_config_versions_scoring_config_version~",
                table: "applications",
                column: "scoring_config_version_id",
                principalTable: "scoring_config_versions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_applications_scoring_config_versions_scoring_config_version~",
                table: "applications");

            migrationBuilder.DropTable(
                name: "scoring_config_versions");

            migrationBuilder.DropIndex(
                name: "IX_applications_scoring_config_version_id",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "scoring_config_version_id",
                table: "applications");
        }
    }
}
