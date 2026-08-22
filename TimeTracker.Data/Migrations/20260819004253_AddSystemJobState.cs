using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemJobState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_job_states",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    job_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_run_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    last_error_message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_job_states", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "uq_system_job_states_job_name",
                table: "system_job_states",
                column: "job_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_job_states");
        }
    }
}
