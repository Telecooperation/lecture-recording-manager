using Microsoft.EntityFrameworkCore.Migrations;

namespace LectureRecordingManager.Migrations
{
    public partial class AddFilePathToRecording : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Recordings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Recordings");
        }
    }
}
