using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace lecture_recording_manager.Migrations
{
    public partial class AddUploadAndPublishDateToRecording : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishDate",
                table: "Recordings",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Sorting",
                table: "Recordings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDate",
                table: "Recordings",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishDate",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "Sorting",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "Recordings");
        }
    }
}
