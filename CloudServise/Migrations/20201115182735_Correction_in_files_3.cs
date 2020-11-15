using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudService_API.Migrations
{
    public partial class Correction_in_files_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LaboratoryWorkId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Disciplines");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Disciplines",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Disciplines",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Disciplines");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Disciplines");

            migrationBuilder.AddColumn<Guid>(
                name: "LaboratoryWorkId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Disciplines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
