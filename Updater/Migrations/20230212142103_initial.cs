using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Updater.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Server = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Locked = table.Column<bool>(type: "bit", nullable: false),
                    SiteUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SitePass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppPoolName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsService = table.Column<bool>(type: "bit", nullable: false),
                    IsPool = table.Column<bool>(type: "bit", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SitePass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatchFilesPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasUpdate = table.Column<bool>(type: "bit", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduleProgress = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Locked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Version",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Patch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Locked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Version", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientUser_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientUser_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VersionFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    File = table.Column<byte[]>(type: "varbinary(900)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersionFile_Version_VersionId",
                        column: x => x.VersionId,
                        principalTable: "Version",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientUser_ClientId",
                table: "ClientUser",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUser_UserId",
                table: "ClientUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VersionFile_File",
                table: "VersionFile",
                column: "File");

            migrationBuilder.CreateIndex(
                name: "IX_VersionFile_VersionId",
                table: "VersionFile",
                column: "VersionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientUser");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "VersionFile");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Version");
        }
    }
}
