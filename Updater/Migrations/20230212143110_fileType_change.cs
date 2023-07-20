using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Updater.Migrations
{
    public partial class fileType_change : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VersionFile_File",
                table: "VersionFile");

            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "VersionFile",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(900)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "File",
                table: "VersionFile",
                type: "varbinary(900)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.CreateIndex(
                name: "IX_VersionFile_File",
                table: "VersionFile",
                column: "File");
        }
    }
}
