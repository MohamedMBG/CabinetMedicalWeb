using CabinetMedicalWeb.Models;
using System;
using System.Collections.Generic;

namespace CabinetMedicalWeb.Areas.Medical.Models
{
    public class MedicalDashboardViewModel
    {
        public ApplicationUser DoctorProfile { get; set; }

        // List for the "Today" section
        public List<RendezVous> AppointmentsToday { get; set; }

        // Dictionary for the "Weekly Calendar" (Key = Date, Value = List of appointments)
        public Dictionary<DateTime, List<RendezVous>> WeekCalendar { get; set; }

        public DateTime WeekStartDate { get; set; }

        public DateTime WeekEndDate { get; set; }

        public DateTime PreviousWeekStart { get; set; }

        public DateTime NextWeekStart { get; set; }

        public List<Conge> WeeklyConges { get; set; }

        public List<Conge> UpcomingConges { get; set; }

        public Conge NewCongeRequest { get; set; }
    }
}