using Microsoft.EntityFrameworkCore.Migrations;

namespace LectureRecordingManager.Migrations
{
    public partial class AddFullHdStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FullHdStatus",
                table: "Recordings",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConvertedPath",
                table: "Lectures",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RenderFullHd",
                table: "Lectures",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullHdStatus",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "ConvertedPath",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "RenderFullHd",
                table: "Lectures");
        }
    }
}
