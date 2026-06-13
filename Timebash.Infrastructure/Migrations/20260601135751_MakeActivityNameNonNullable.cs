using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timebash.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeActivityNameNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Activities\" SET \"Name\" = '' WHERE \"Name\" IS NULL;");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Activities",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Activities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
