using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudService_API.Migrations
{
    public partial class Correction_in_files_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Patch",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "LaboratoryWorkId",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathToDirectory",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PathToFile",
                table: "Files",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaboratoryWorkId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "PathToDirectory",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "PathToFile",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "Patch",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
