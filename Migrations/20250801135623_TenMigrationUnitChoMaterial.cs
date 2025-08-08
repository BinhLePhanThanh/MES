using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class TenMigrationUnitChoMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Materials",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_UnitId",
                table: "Materials",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Units_UnitId",
                table: "Materials",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Units_UnitId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_UnitId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Materials");
        }
    }
}
