using System;
using System.Collections.Generic;

namespace CabinetMedicalWeb.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int PatientsCount { get; set; }
        public int DoctorsCount { get; set; }
        public int MedicalFoldersCount { get; set; }
        public int PrescriptionsToday { get; set; }
        public IEnumerable<DailyPrescriptionStat> PrescriptionsByDay { get; set; } = new List<DailyPrescriptionStat>();
        public IEnumerable<PatientReservationPoint> WeeklyPatientReservations { get; set; } = new List<PatientReservationPoint>();
        public IEnumerable<PatientReservationPoint> MonthlyPatientReservations { get; set; } = new List<PatientReservationPoint>();
        public IEnumerable<UserPerformanceStat> DoctorPerformance { get; set; } = new List<UserPerformanceStat>();
        public ReservationWorkflowPerformance ReceptionPerformance { get; set; } = new ReservationWorkflowPerformance();
    }

    public class DailyPrescriptionStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class PatientReservationPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Patients { get; set; }
        public int Reservations { get; set; }
    }

    public class UserPerformanceStat
    {
        public string Name { get; set; } = string.Empty;
        public string? Specialite { get; set; }
        public int Appointments { get; set; }
        public int Prescriptions { get; set; }
    }

    public class ReservationWorkflowPerformance
    {
        public int Confirmed { get; set; }
        public int Rejected { get; set; }
        public int Pending { get; set; }
    }
}
