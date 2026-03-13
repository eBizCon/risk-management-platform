using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RiskManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    income = table.Column<double>(type: "double precision", nullable: false),
                    fixed_costs = table.Column<double>(type: "double precision", nullable: false),
                    desired_rate = table.Column<double>(type: "double precision", nullable: false),
                    employment_status = table.Column<string>(type: "text", nullable: false),
                    has_payment_default = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "draft"),
                    score = table.Column<int>(type: "integer", nullable: true),
                    traffic_light = table.Column<string>(type: "text", nullable: true),
                    scoring_reasons = table.Column<string>(type: "text", nullable: true),
                    processor_comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_inquiries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    inquiry_text = table.Column<string>(type: "text", nullable: false),
                    response_text = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "open"),
                    processor_email = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_inquiries", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_inquiries_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_application_inquiries_application_id",
                table: "application_inquiries",
                column: "application_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_inquiries");

            migrationBuilder.DropTable(
                name: "applications");
        }
    }
}
