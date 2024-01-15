using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class updatemodulemodelandserialnumbermodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNo",
                table: "ModuleDb");

            migrationBuilder.CreateTable(
                name: "SerialNumberDb",
                columns: table => new
                {
                    SNID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumberDb", x => x.SNID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialNumberDb");

            migrationBuilder.AddColumn<string>(
                name: "SerialNo",
                table: "ModuleDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
