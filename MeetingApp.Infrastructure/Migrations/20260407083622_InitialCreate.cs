using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colleagues",
                columns: table => new
                {
                    EntraObjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ProfilePictureUri = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ModerationCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsManuallyAdded = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colleagues", x => x.EntraObjectId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colleagues_Department_IsActive",
                table: "Colleagues",
                columns: new[] { "Department", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Colleagues_Email",
                table: "Colleagues",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colleagues");
        }
    }
}
