using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class addwsdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkmanshipStandard",
                table: "MTIDb");

            migrationBuilder.CreateTable(
                name: "WSDb",
                columns: table => new
                {
                    WSID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkmanshipStandardDoc = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    RevNo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WSDb", x => x.WSID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WSDb");

            migrationBuilder.AddColumn<string>(
                name: "WorkmanshipStandard",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
