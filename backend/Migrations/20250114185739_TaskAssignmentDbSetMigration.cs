using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class TaskAssignmentDbSetMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tasks_assignments",
                columns: table => new
                {
                    task_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    assignee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assignee_type = table.Column<int>(type: "int", unicode: false, maxLength: 15, nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks_assignments", x => new { x.task_id, x.assignee_id, x.assignee_type });
                    table.ForeignKey(
                        name: "FK_tasks_assignments_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasks_assignments");
        }
    }
}
