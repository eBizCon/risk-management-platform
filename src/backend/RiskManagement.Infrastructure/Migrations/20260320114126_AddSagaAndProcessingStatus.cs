using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaAndProcessingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "applications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "saga_application_creation_state",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    income = table.Column<double>(type: "double precision", nullable: false),
                    fixed_costs = table.Column<double>(type: "double precision", nullable: false),
                    desired_rate = table.Column<double>(type: "double precision", nullable: false),
                    user_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    auto_submit = table.Column<bool>(type: "boolean", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    employment_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    date_of_birth = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    has_payment_default = table.Column<bool>(type: "boolean", nullable: true),
                    credit_score = table.Column<int>(type: "integer", nullable: true),
                    credit_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    credit_provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saga_application_creation_state", x => x.correlation_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saga_application_creation_state");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "applications");
        }
    }
}
