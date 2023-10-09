using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class addownerreandvali : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerRemarks",
                table: "PLDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Validation",
                table: "PLDb",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerRemarks",
                table: "PLDb");

            migrationBuilder.DropColumn(
                name: "Validation",
                table: "PLDb");
        }
    }
}
