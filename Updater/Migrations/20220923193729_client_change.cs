using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Updater.Migrations
{
    public partial class client_change : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppPoolName",
                table: "Client",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "Client",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SitePass",
                table: "Client",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SiteUser",
                table: "Client",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppPoolName",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "SitePass",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "SiteUser",
                table: "Client");
        }
    }
}
