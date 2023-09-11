using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMD_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class MTIModelRev1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Documnet1",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "Documnet2",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "Documnet3",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "Documnet4",
                table: "MTIDb");

            migrationBuilder.RenameColumn(
                name: "Numberchuchu",
                table: "MTIDb",
                newName: "WorkmanshipStandard");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyDesc",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyDrawing",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyPN",
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

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "MTIDb",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Derogation",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
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

            migrationBuilder.AddColumn<bool>(
                name: "ObsoleteStat",
                table: "MTIDb",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OnePointLesson",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginatorName",
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

            migrationBuilder.AddColumn<string>(
                name: "RevNo",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SchematicDiagram",
                table: "MTIDb",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssemblyDesc",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "AssemblyDrawing",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "AssemblyPN",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "BillsOfMaterial",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "Derogation",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "EngrMemo",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "ObsoleteStat",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "OnePointLesson",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "OriginatorName",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "PRCO",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "RevNo",
                table: "MTIDb");

            migrationBuilder.DropColumn(
                name: "SchematicDiagram",
                table: "MTIDb");

            migrationBuilder.RenameColumn(
                name: "WorkmanshipStandard",
                table: "MTIDb",
                newName: "Numberchuchu");

            migrationBuilder.AddColumn<byte[]>(
                name: "Documnet1",
                table: "MTIDb",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Documnet2",
                table: "MTIDb",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Documnet3",
                table: "MTIDb",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Documnet4",
                table: "MTIDb",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
