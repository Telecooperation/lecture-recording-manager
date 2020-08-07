using Microsoft.EntityFrameworkCore.Migrations;

namespace LectureRecordingManager.Migrations
{
    public partial class AddCustomTargetName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomTargetName",
                table: "Recordings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomTargetName",
                table: "Recordings");
        }
    }
}
