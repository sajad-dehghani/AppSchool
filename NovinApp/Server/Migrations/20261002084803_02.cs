using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovinApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class _02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CounselingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentUserId = table.Column<int>(type: "int", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ConsultantUserId = table.Column<int>(type: "int", nullable: true),
                    ConsultantName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreferredDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreferredTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConsultantResponse = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ScheduledDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ScheduledTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedStudyPlanId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounselingRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantUserId = table.Column<int>(type: "int", nullable: false),
                    ConsultantName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    StudentUserId = table.Column<int>(type: "int", nullable: false),
                    StudentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    PersianDate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GregorianDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TestCount = table.Column<int>(type: "int", nullable: true),
                    TestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GradeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Chapter = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ConsultantNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConsultantAttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConsultantAttachmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsCounselingSession = table.Column<bool>(type: "bit", nullable: false),
                    CounselingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsFeedbackSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    FeedbackSubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QualityRating = table.Column<int>(type: "int", nullable: true),
                    QualityText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StudyPercentage = table.Column<int>(type: "int", nullable: true),
                    CorrectTests = table.Column<int>(type: "int", nullable: true),
                    WrongTests = table.Column<int>(type: "int", nullable: true),
                    UnansweredTests = table.Column<int>(type: "int", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    StudentNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StudentAttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StudentAttachmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TestSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlanItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudySubjectTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FieldOfStudy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Chapter = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriorityOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySubjectTopics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounselingRequests_ConsultantUserId",
                table: "CounselingRequests",
                column: "ConsultantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingRequests_Status",
                table: "CounselingRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CounselingRequests_StudentUserId",
                table: "CounselingRequests",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanItems_ActivityType",
                table: "StudyPlanItems",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanItems_ConsultantUserId",
                table: "StudyPlanItems",
                column: "ConsultantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanItems_GregorianDate",
                table: "StudyPlanItems",
                column: "GregorianDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanItems_PersianDate",
                table: "StudyPlanItems",
                column: "PersianDate");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanItems_Status",
                table: "StudyPlanItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlanItems_StudentUserId",
                table: "StudyPlanItems",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjectTopics_FieldOfStudy",
                table: "StudySubjectTopics",
                column: "FieldOfStudy");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjectTopics_GradeLevel",
                table: "StudySubjectTopics",
                column: "GradeLevel");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjectTopics_IsActive",
                table: "StudySubjectTopics",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjectTopics_Subject",
                table: "StudySubjectTopics",
                column: "Subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounselingRequests");

            migrationBuilder.DropTable(
                name: "StudyPlanItems");

            migrationBuilder.DropTable(
                name: "StudySubjectTopics");
        }
    }
}
