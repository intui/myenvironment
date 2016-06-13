using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace myEnvironment.Migrations
{
    public partial class MyFirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Setup",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Adress = table.Column<string>(nullable: true),
                    InstallDate = table.Column<DateTime>(nullable: false),
                    SensorType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    EnvDatasetId = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Humidity = table.Column<decimal>(nullable: false),
                    SetupId = table.Column<int>(nullable: true),
                    StimeStamp = table.Column<DateTime>(nullable: false),
                    Temperature = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.EnvDatasetId);
                    table.ForeignKey(
                        name: "FK_Posts_Setup_SetupId",
                        column: x => x.SetupId,
                        principalTable: "Setup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_SetupId",
                table: "Posts",
                column: "SetupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Setup");
        }
    }
}
