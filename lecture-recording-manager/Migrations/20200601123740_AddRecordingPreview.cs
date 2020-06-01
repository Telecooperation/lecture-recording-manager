using Microsoft.EntityFrameworkCore.Migrations;

namespace LectureRecordingManager.Migrations
{
    public partial class AddRecordingPreview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Preview",
                table: "Recordings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preview",
                table: "Recordings");
        }
    }
}
