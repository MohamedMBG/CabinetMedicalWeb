using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinetMedicalWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateSouhaitee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motif = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Statut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateHeureConfirmee = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationRequests_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReservationRequests_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationRequests_DoctorId",
                table: "ReservationRequests",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationRequests_PatientId",
                table: "ReservationRequests",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationRequests");
        }
    }
}
