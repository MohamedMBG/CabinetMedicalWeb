using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CabinetMedicalWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalModels_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultation_Dossiers_DossierMedicalId",
                table: "Consultation");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Dossiers_DossierMedicalId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultatExamens_Dossiers_DossierMedicalId",
                table: "ResultatExamens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dossiers",
                table: "Dossiers");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Dossiers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Motif",
                table: "Consultation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dossiers",
                table: "Dossiers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Dossiers_PatientId",
                table: "Dossiers",
                column: "PatientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultation_Dossiers_DossierMedicalId",
                table: "Consultation",
                column: "DossierMedicalId",
                principalTable: "Dossiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Dossiers_DossierMedicalId",
                table: "Prescriptions",
                column: "DossierMedicalId",
                principalTable: "Dossiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultatExamens_Dossiers_DossierMedicalId",
                table: "ResultatExamens",
                column: "DossierMedicalId",
                principalTable: "Dossiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultation_Dossiers_DossierMedicalId",
                table: "Consultation");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Dossiers_DossierMedicalId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultatExamens_Dossiers_DossierMedicalId",
                table: "ResultatExamens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dossiers",
                table: "Dossiers");

            migrationBuilder.DropIndex(
                name: "IX_Dossiers_PatientId",
                table: "Dossiers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Dossiers");

            migrationBuilder.DropColumn(
                name: "Motif",
                table: "Consultation");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dossiers",
                table: "Dossiers",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultation_Dossiers_DossierMedicalId",
                table: "Consultation",
                column: "DossierMedicalId",
                principalTable: "Dossiers",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Dossiers_DossierMedicalId",
                table: "Prescriptions",
                column: "DossierMedicalId",
                principalTable: "Dossiers",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultatExamens_Dossiers_DossierMedicalId",
                table: "ResultatExamens",
                column: "DossierMedicalId",
                principalTable: "Dossiers",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
