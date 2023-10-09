using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class addownerinpl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PLDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PLDb");
        }
    }
}
