using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OhneGraph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Colleagues_Department_IsActive",
                table: "Colleagues");

            migrationBuilder.DropIndex(
                name: "IX_Colleagues_Email",
                table: "Colleagues");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Colleagues");

            migrationBuilder.DropColumn(
                name: "IsManuallyAdded",
                table: "Colleagues");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUri",
                table: "Colleagues");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Colleagues",
                newName: "Upn");

            migrationBuilder.RenameColumn(
                name: "EntraObjectId",
                table: "Colleagues",
                newName: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Colleagues_Upn",
                table: "Colleagues",
                column: "Upn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Colleagues_Upn",
                table: "Colleagues");

            migrationBuilder.RenameColumn(
                name: "Upn",
                table: "Colleagues",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Colleagues",
                newName: "EntraObjectId");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Colleagues",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyAdded",
                table: "Colleagues",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUri",
                table: "Colleagues",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colleagues_Department_IsActive",
                table: "Colleagues",
                columns: new[] { "Department", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Colleagues_Email",
                table: "Colleagues",
                column: "Email");
        }
    }
}
