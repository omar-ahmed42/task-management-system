using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class TeamsCascadeMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_users_leader_id",
                table: "teams");

            migrationBuilder.AlterColumn<Guid>(
                name: "leader_id",
                table: "teams",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_id",
                table: "teams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_by_id",
                table: "teams",
                column: "created_by_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_users_created_by_id",
                table: "teams",
                column: "created_by_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_users_leader_id",
                table: "teams",
                column: "leader_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_users_created_by_id",
                table: "teams");

            migrationBuilder.DropForeignKey(
                name: "FK_teams_users_leader_id",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_created_by_id",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "teams");

            migrationBuilder.AlterColumn<Guid>(
                name: "leader_id",
                table: "teams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_teams_users_leader_id",
                table: "teams",
                column: "leader_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
