using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class AddPLDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PLDb",
                columns: table => new
                {
                    PLID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PLNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedDoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Product = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PNDN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Problem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reporter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterimDoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDTCD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IDStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardizedDoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SDTCD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SDStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Validator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PLIDStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PLSDStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PLRemarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PLDb", x => x.PLID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PLDb");
        }
    }
}
