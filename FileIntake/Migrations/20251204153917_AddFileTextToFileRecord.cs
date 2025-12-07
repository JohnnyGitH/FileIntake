using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileIntake.Migrations
{
    /// <inheritdoc />
    public partial class AddFileTextToFileRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileText",
                table: "Files",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileText",
                table: "Files");
        }
    }
}
