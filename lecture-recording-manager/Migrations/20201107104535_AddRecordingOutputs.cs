using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace LectureRecordingManager.Migrations
{
    public partial class AddRecordingOutputs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullHdStatus",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "Preview",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Recordings");

            migrationBuilder.DropColumn(
                name: "StatusText",
                table: "Recordings");

            migrationBuilder.CreateTable(
                name: "RecordingOutputs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecordingId = table.Column<int>(nullable: false),
                    JobType = table.Column<string>(nullable: true),
                    JobConfiguration = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Processed = table.Column<bool>(nullable: false),
                    JobError = table.Column<string>(nullable: true),
                    DateStarted = table.Column<DateTimeOffset>(nullable: true),
                    DateFinished = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordingOutputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordingOutputs_Recordings_RecordingId",
                        column: x => x.RecordingId,
                        principalTable: "Recordings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecordingOutputs_RecordingId",
                table: "RecordingOutputs",
                column: "RecordingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecordingOutputs");

            migrationBuilder.AddColumn<int>(
                name: "FullHdStatus",
                table: "Recordings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Preview",
                table: "Recordings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Recordings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusText",
                table: "Recordings",
                type: "text",
                nullable: true);
        }
    }
}
