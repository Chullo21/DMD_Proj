using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class adddocctrlnorevnono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogsheetDocNo",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogsheetRevNo",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogsheetDocNo",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "LogsheetRevNo",
                table: "MTIDb");
        }
    }
}
