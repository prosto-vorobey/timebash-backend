using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timebash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfomanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Activities_JournalId",
                table: "Activities");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_JournalId_EndTime_StartTime",
                table: "Activities",
                columns: new[] { "JournalId", "EndTime", "StartTime" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_JournalId_StartTime_EndTime",
                table: "Activities",
                columns: new[] { "JournalId", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Activities_JournalId_EndTime_StartTime",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_JournalId_StartTime_EndTime",
                table: "Activities");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_JournalId",
                table: "Activities",
                column: "JournalId");
        }
    }
}
