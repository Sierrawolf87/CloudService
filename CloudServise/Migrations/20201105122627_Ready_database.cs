using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudServise_API.Migrations
{
    public partial class Ready_database : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaboratoryWorks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    OwnerId = table.Column<Guid>(nullable: false),
                    DisciplineId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboratoryWorks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaboratoryWorks_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    LaboratoryWorkId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requirements_LaboratoryWorks_LaboratoryWorkId",
                        column: x => x.LaboratoryWorkId,
                        principalTable: "LaboratoryWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    OwnerId = table.Column<Guid>(nullable: false),
                    Mark = table.Column<int>(nullable: false),
                    LaboratoryWorkId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solutions_LaboratoryWorks_LaboratoryWorkId",
                        column: x => x.LaboratoryWorkId,
                        principalTable: "LaboratoryWorks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Patch = table.Column<string>(nullable: true),
                    OwnerId = table.Column<Guid>(nullable: false),
                    RequirementId = table.Column<Guid>(nullable: true),
                    SolutionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Requirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "Requirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Files_Solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "Solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_RequirementId",
                table: "Files",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_SolutionId",
                table: "Files",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratoryWorks_DisciplineId",
                table: "LaboratoryWorks",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_LaboratoryWorkId",
                table: "Requirements",
                column: "LaboratoryWorkId");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_LaboratoryWorkId",
                table: "Solutions",
                column: "LaboratoryWorkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Requirements");

            migrationBuilder.DropTable(
                name: "Solutions");

            migrationBuilder.DropTable(
                name: "LaboratoryWorks");
        }
    }
}
