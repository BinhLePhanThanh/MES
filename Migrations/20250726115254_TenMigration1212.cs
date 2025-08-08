using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class TenMigration1212 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FormulaLogs_FormulaId",
                table: "FormulaLogs",
                column: "FormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaLogs_PerformedById",
                table: "FormulaLogs",
                column: "PerformedById");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaLogs_Employees_PerformedById",
                table: "FormulaLogs",
                column: "PerformedById",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormulaLogs_Formulas_FormulaId",
                table: "FormulaLogs",
                column: "FormulaId",
                principalTable: "Formulas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormulaLogs_Employees_PerformedById",
                table: "FormulaLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_FormulaLogs_Formulas_FormulaId",
                table: "FormulaLogs");

            migrationBuilder.DropIndex(
                name: "IX_FormulaLogs_FormulaId",
                table: "FormulaLogs");

            migrationBuilder.DropIndex(
                name: "IX_FormulaLogs_PerformedById",
                table: "FormulaLogs");
        }
    }
}
