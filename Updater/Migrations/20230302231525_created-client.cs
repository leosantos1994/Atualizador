using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Updater.Migrations
{
    public partial class createdclient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Creation",
                table: "Client",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creation",
                table: "Client");
        }
    }
}
