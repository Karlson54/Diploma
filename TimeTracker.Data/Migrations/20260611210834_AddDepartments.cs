using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
    // 1. Сначала создаём таблицу departments
    migrationBuilder.CreateTable(
        name: "departments",
        columns: table => new
        {
            id = table.Column<long>(type: "bigint", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
            agency_id = table.Column<long>(type: "bigint", nullable: false),
            created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
            updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_departments", x => x.id);
            table.ForeignKey(
                name: "fk_departments_agency_id",
                column: x => x.agency_id,
                principalTable: "agencies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateIndex(
        name: "ix_departments_agency_id",
        table: "departments",
        column: "agency_id");

    migrationBuilder.CreateIndex(
        name: "ix_departments_is_active",
        table: "departments",
        column: "is_active");

    migrationBuilder.CreateIndex(
        name: "uq_departments_agency_name",
        table: "departments",
        columns: new[] { "agency_id", "name" },
        unique: true);

    // 2. Вставляем дефолтный департамент для каждого agency
    migrationBuilder.Sql(@"
        INSERT INTO departments (name, is_active, agency_id, created_at)
        SELECT 'Default', 1, id, GETDATE()
        FROM agencies
    ");

    // 3. Добавляем колонки как nullable сначала
    migrationBuilder.AddColumn<long>(
        name: "department_id",
        table: "users",
        type: "bigint",
        nullable: true);

    migrationBuilder.AddColumn<long>(
        name: "department_id",
        table: "time_entries",
        type: "bigint",
        nullable: true);

    // 4. Заполняем department_id для существующих строк
    migrationBuilder.Sql(@"
        UPDATE u
        SET u.department_id = d.id
        FROM users u
        INNER JOIN departments d ON d.agency_id = u.agency_id
    ");

    migrationBuilder.Sql(@"
        UPDATE te
        SET te.department_id = d.id
        FROM time_entries te
        INNER JOIN departments d ON d.agency_id = te.agency_id
    ");

    // 5. Делаем колонки NOT NULL
    migrationBuilder.AlterColumn<long>(
        name: "department_id",
        table: "users",
        type: "bigint",
        nullable: false,
        oldClrType: typeof(long),
        oldType: "bigint",
        oldNullable: true);

    migrationBuilder.AlterColumn<long>(
        name: "department_id",
        table: "time_entries",
        type: "bigint",
        nullable: false,
        oldClrType: typeof(long),
        oldType: "bigint",
        oldNullable: true);

    // 6. Добавляем индексы и FK
    migrationBuilder.CreateIndex(
        name: "ix_users_department_id",
        table: "users",
        column: "department_id");

    migrationBuilder.CreateIndex(
        name: "ix_time_entries_department_id",
        table: "time_entries",
        column: "department_id");

    migrationBuilder.AddForeignKey(
        name: "fk_time_entries_department_id",
        table: "time_entries",
        column: "department_id",
        principalTable: "departments",
        principalColumn: "id",
        onDelete: ReferentialAction.Restrict);

    migrationBuilder.AddForeignKey(
        name: "fk_users_department_id",
        table: "users",
        column: "department_id",
        principalTable: "departments",
        principalColumn: "id",
        onDelete: ReferentialAction.Restrict);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_time_entries_department_id",
                table: "time_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_users_department_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropIndex(
                name: "ix_users_department_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_time_entries_department_id",
                table: "time_entries");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "time_entries");
        }
    }
}
