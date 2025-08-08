using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class TenMigrationEmpFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Employees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedById",
                table: "Employees",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_CreatedById",
                table: "Employees",
                column: "CreatedById",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_CreatedById",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CreatedById",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Employees");
        }
    }
}
