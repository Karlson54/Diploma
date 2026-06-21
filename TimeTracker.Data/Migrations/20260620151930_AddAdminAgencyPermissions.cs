using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAgencyPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_agency_permissions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    agency_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by_user_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_agency_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_admin_agency_permissions_agency_id",
                        column: x => x.agency_id,
                        principalTable: "agencies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_admin_agency_permissions_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_admin_agency_permissions_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_admin_agency_permissions_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_agency_permissions_agency_id",
                table: "admin_agency_permissions",
                column: "agency_id");

            migrationBuilder.CreateIndex(
                name: "ix_admin_agency_permissions_created_by_user_id",
                table: "admin_agency_permissions",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_admin_agency_permissions_department_id",
                table: "admin_agency_permissions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_admin_agency_permissions_user_id",
                table: "admin_agency_permissions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_admin_agency_permissions",
                table: "admin_agency_permissions",
                columns: new[] { "user_id", "agency_id", "department_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_agency_permissions");
        }
    }
}