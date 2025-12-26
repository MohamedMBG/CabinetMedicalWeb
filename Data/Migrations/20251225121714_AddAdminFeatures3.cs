using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinetMedicalWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFeatures3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Conges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Conges");
        }
    }
}
