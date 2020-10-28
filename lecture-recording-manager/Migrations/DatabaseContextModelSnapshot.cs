﻿// <auto-generated />
using System;
using LectureRecordingManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace LectureRecordingManager.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("LectureRecordingManager.Models.Lecture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<string>("ConvertedPath")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("LastSynchronized")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Publish")
                        .HasColumnType("boolean");

                    b.Property<string>("PublishPath")
                        .HasColumnType("text");

                    b.Property<bool>("RenderFullHd")
                        .HasColumnType("boolean");

                    b.Property<int>("SemesterId")
                        .HasColumnType("integer");

                    b.Property<string>("SourcePath")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("SemesterId");

                    b.ToTable("Lectures");
                });

            modelBuilder.Entity("LectureRecordingManager.Models.Recording", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("CustomTargetName")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<double>("Duration")
                        .HasColumnType("double precision");

                    b.Property<string>("FilePath")
                        .HasColumnType("text");

                    b.Property<int>("FullHdStatus")
                        .HasColumnType("integer");

                    b.Property<int>("LectureId")
                        .HasColumnType("integer");

                    b.Property<bool>("Preview")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("PublishDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Published")
                        .HasColumnType("boolean");

                    b.Property<int>("Sorting")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("StatusText")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("UploadDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("LectureId");

                    b.ToTable("Recordings");
                });

            modelBuilder.Entity("LectureRecordingManager.Models.RecordingChapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("RecordingId")
                        .HasColumnType("integer");

                    b.Property<double>("StartPosition")
                        .HasColumnType("double precision");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RecordingId");

                    b.ToTable("RecordingChapters");
                });

            modelBuilder.Entity("LectureRecordingManager.Models.Semester", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset>("DateEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("DateStart")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("Published")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Semesters");
                });

            modelBuilder.Entity("LectureRecordingManager.Models.Lecture", b =>
                {
                    b.HasOne("LectureRecordingManager.Models.Semester", "Semester")
                        .WithMany()
                        .HasForeignKey("SemesterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LectureRecordingManager.Models.Recording", b =>
                {
                    b.HasOne("LectureRecordingManager.Models.Lecture", "Lecture")
                        .WithMany()
                        .HasForeignKey("LectureId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LectureRecordingManager.Models.RecordingChapter", b =>
                {
                    b.HasOne("LectureRecordingManager.Models.Recording", "Recording")
                        .WithMany("Chapters")
                        .HasForeignKey("RecordingId");
                });
#pragma warning restore 612, 618
        }
    }
}
