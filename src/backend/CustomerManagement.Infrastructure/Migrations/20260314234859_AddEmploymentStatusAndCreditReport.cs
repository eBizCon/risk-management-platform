using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentStatusAndCreditReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "credit_report_checked_at",
                schema: "customer",
                table: "customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "credit_report_credit_score",
                schema: "customer",
                table: "customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "credit_report_has_payment_default",
                schema: "customer",
                table: "customers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "credit_report_provider",
                schema: "customer",
                table: "customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "employment_status",
                schema: "customer",
                table: "customers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "employed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "credit_report_checked_at",
                schema: "customer",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "credit_report_credit_score",
                schema: "customer",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "credit_report_has_payment_default",
                schema: "customer",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "credit_report_provider",
                schema: "customer",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "employment_status",
                schema: "customer",
                table: "customers");
        }
    }
}
