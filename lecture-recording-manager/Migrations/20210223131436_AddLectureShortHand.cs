using Microsoft.EntityFrameworkCore.Migrations;

namespace LectureRecordingManager.Migrations
{
    public partial class AddLectureShortHand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortHand",
                table: "Lectures",
                type: "varchar(255)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortHand",
                table: "Lectures");
        }
    }
}
