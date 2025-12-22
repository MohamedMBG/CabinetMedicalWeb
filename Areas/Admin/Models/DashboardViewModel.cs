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
    }

    public class DailyPrescriptionStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
