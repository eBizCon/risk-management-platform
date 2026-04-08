using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditScoreScoringParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "credit_score_good",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "credit_score_moderate",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_low_credit_score",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_moderate_credit_score",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "credit_score_good",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "credit_score_moderate",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_low_credit_score",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_moderate_credit_score",
                table: "scoring_config_versions");
        }
    }
}
