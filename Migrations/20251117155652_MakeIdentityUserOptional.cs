using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileIntake.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdentityUserOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "UserProfiles",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityUserId",
                table: "UserProfiles",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
