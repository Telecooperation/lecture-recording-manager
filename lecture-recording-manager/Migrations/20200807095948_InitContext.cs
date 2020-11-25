using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace LectureRecordingManager.Migrations
{
    public partial class InitContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(255)", nullable: true),
                    DateStart = table.Column<DateTimeOffset>(nullable: false),
                    DateEnd = table.Column<DateTimeOffset>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lectures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "varchar(255)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    SemesterId = table.Column<int>(nullable: false),
                    SourcePath = table.Column<string>(nullable: true),
                    PublishPath = table.Column<string>(nullable: true),
                    LastSynchronized = table.Column<DateTimeOffset>(nullable: true),
                    Publish = table.Column<bool>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lectures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lectures_Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recordings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LectureId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Title = table.Column<string>(type: "varchar(255)", nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Duration = table.Column<double>(nullable: false),
                    Published = table.Column<bool>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    StatusText = table.Column<string>(nullable: true),
                    UploadDate = table.Column<DateTimeOffset>(nullable: true),
                    PublishDate = table.Column<DateTimeOffset>(nullable: true),
                    Sorting = table.Column<int>(nullable: false),
                    Preview = table.Column<bool>(nullable: false),
                    FilePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recordings_Lectures_LectureId",
                        column: x => x.LectureId,
                        principalTable: "Lectures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordingChapters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecordingId = table.Column<int>(nullable: true),
                    StartPosition = table.Column<double>(nullable: false),
                    Thumbnail = table.Column<string>(nullable: true),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordingChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordingChapters_Recordings_RecordingId",
                        column: x => x.RecordingId,
                        principalTable: "Recordings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_SemesterId",
                table: "Lectures",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordingChapters_RecordingId",
                table: "RecordingChapters",
                column: "RecordingId");

            migrationBuilder.CreateIndex(
                name: "IX_Recordings_LectureId",
                table: "Recordings",
                column: "LectureId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecordingChapters");

            migrationBuilder.DropTable(
                name: "Recordings");

            migrationBuilder.DropTable(
                name: "Lectures");

            migrationBuilder.DropTable(
                name: "Semesters");
        }
    }
}
