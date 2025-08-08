using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class StorageUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormulaFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormulaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedById = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormulaFiles_Employees_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FormulaFiles_Formulas_FormulaId",
                        column: x => x.FormulaId,
                        principalTable: "Formulas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormulaFiles_FormulaId",
                table: "FormulaFiles",
                column: "FormulaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormulaFiles_UploadedById",
                table: "FormulaFiles",
                column: "UploadedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulaFiles");
        }
    }
}
