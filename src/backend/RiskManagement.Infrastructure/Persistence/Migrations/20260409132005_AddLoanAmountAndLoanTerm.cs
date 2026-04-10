using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanAmountAndLoanTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "loan_term_long",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "loan_term_medium",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "loan_term_short",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "loan_to_income_ratio_good",
                table: "scoring_config_versions",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "loan_to_income_ratio_high",
                table: "scoring_config_versions",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "loan_to_income_ratio_moderate",
                table: "scoring_config_versions",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "penalty_critical_loan_to_income",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_high_loan_to_income",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_long_loan_term",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_medium_loan_term",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_moderate_loan_to_income",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "penalty_very_long_loan_term",
                table: "scoring_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "loan_amount",
                table: "saga_application_creation_state",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "loan_term",
                table: "saga_application_creation_state",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "loan_amount",
                table: "applications",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "loan_term",
                table: "applications",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "loan_term_long",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "loan_term_medium",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "loan_term_short",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "loan_to_income_ratio_good",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "loan_to_income_ratio_high",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "loan_to_income_ratio_moderate",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_critical_loan_to_income",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_high_loan_to_income",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_long_loan_term",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_medium_loan_term",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_moderate_loan_to_income",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "penalty_very_long_loan_term",
                table: "scoring_config_versions");

            migrationBuilder.DropColumn(
                name: "loan_amount",
                table: "saga_application_creation_state");

            migrationBuilder.DropColumn(
                name: "loan_term",
                table: "saga_application_creation_state");

            migrationBuilder.DropColumn(
                name: "loan_amount",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "loan_term",
                table: "applications");
        }
    }
}
