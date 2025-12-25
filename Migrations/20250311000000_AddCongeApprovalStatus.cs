using Microsoft.EntityFrameworkCore.Migrations;

namespace CabinetMedicalWeb.Migrations
{
    [DbContext(typeof(CabinetMedicalWeb.Data.ApplicationDbContext))]
    [Migration("20250311000000_AddCongeApprovalStatus")]
    public partial class AddCongeApprovalStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Conges",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Conges");
        }
    }
}
