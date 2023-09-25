using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class adddoctype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssemblyDrawing",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "BillsOfMaterial",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "Derogation",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "EngrMemo",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "OnePointLesson",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "PRCO",
                table: "MTIDb");

            migrationBuilder.RenameColumn(
                name: "SchematicDiagram",
                table: "MTIDb",
                newName: "DocType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocType",
                table: "MTIDb",
                newName: "SchematicDiagram");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyDrawing",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillsOfMaterial",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Derogation",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EngrMemo",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OnePointLesson",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PRCO",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
