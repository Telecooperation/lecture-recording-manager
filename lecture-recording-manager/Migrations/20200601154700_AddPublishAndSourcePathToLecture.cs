using Microsoft.EntityFrameworkCore.Migrations;

namespace LectureRecordingManager.Migrations
{
    public partial class AddPublishAndSourcePathToLecture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublishPath",
                table: "Lectures",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourcePath",
                table: "Lectures",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishPath",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "SourcePath",
                table: "Lectures");
        }
    }
}
