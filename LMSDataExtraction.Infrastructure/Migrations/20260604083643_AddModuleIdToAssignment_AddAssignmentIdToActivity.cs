using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSDataExtraction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleIdToAssignment_AddAssignmentIdToActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Assignments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignmentId",
                table: "Activities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ModuleId",
                table: "Assignments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_AssignmentId",
                table: "Activities",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Assignments_AssignmentId",
                table: "Activities",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Modules_ModuleId",
                table: "Assignments",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Assignments_AssignmentId",
                table: "Activities");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Modules_ModuleId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_ModuleId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Activities_AssignmentId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Activities");
        }
    }
}
