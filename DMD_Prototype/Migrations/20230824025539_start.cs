using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsDb",
                columns: table => new
                {
                    InsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InsPhoto = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Page = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsDb", x => x.InsID);
                });

            migrationBuilder.CreateTable(
                name: "MTIDb",
                columns: table => new
                {
                    MTIID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Documnet1 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Documnet2 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Documnet3 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Documnet4 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Numberchuchu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTIDb", x => x.MTIID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsDb");

            migrationBuilder.DropTable(
                name: "MTIDb");
        }
    }
}
