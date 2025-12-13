using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinetMedicalWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalTablesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Horaires",
                columns: table => new
                {
                    IdHoraire = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JoursTravail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeuresTravail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horaires", x => x.IdHoraire);
                    table.ForeignKey(
                        name: "FK_Horaires_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    IdPrescription = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatePrescription = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Medicaments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DossierMedicalId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.IdPrescription);
                    table.ForeignKey(
                        name: "FK_Prescriptions_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Dossiers_DossierMedicalId",
                        column: x => x.DossierMedicalId,
                        principalTable: "Dossiers",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultatExamens",
                columns: table => new
                {
                    IdResultat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateExamen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeExamen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DossierMedicalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultatExamens", x => x.IdResultat);
                    table.ForeignKey(
                        name: "FK_ResultatExamens_Dossiers_DossierMedicalId",
                        column: x => x.DossierMedicalId,
                        principalTable: "Dossiers",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Horaires_DoctorId",
                table: "Horaires",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DossierMedicalId",
                table: "Prescriptions",
                column: "DossierMedicalId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultatExamens_DossierMedicalId",
                table: "ResultatExamens",
                column: "DossierMedicalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Horaires");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "ResultatExamens");
        }
    }
}
