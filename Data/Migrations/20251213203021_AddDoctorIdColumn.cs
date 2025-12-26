using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinetMedicalWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorId",
                table: "Consultation",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultation_DoctorId",
                table: "Consultation",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultation_AspNetUsers_DoctorId",
                table: "Consultation",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultation_AspNetUsers_DoctorId",
                table: "Consultation");

            migrationBuilder.DropIndex(
                name: "IX_Consultation_DoctorId",
                table: "Consultation");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Consultation");
        }
    }
}
