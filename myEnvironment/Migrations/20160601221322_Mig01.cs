using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace myEnvironment.Migrations
{
    public partial class Mig01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    SensorId = table.Column<Guid>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.SensorId);
                });

            migrationBuilder.CreateTable(
                name: "AmbientDataSample",
                columns: table => new
                {
                    AmbienceId = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", false),
                    SensorId = table.Column<Guid>(nullable: false),
                    captureTime = table.Column<DateTime>(nullable: false),
                    humidity = table.Column<decimal>(nullable: false),
                    temperature = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmbientDataSample", x => x.AmbienceId);
                    table.ForeignKey(
                        name: "FK_AmbientDataSample_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "SensorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmbientDataSample_SensorId",
                table: "AmbientDataSample",
                column: "SensorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmbientDataSample");

            migrationBuilder.DropTable(
                name: "Sensors");
        }
    }
}
