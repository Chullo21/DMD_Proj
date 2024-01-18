using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class addannouncementdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnnouncementDb",
                columns: table => new
                {
                    AnnouncementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnouncementCreator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAnnounced = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementDb", x => x.AnnouncementID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnouncementDb");
        }
    }
}
