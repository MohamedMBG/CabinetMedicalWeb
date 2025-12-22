namespace CabinetMedicalWeb.Areas.Medical.Models
{
    public class PatientListItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string DateNaissance { get; set; }

        public PatientListItem(int id, string fullName, string telephone, string email, string dateNaissance)
        {
            Id = id;
            FullName = fullName;
            Telephone = telephone;
            Email = email;
            DateNaissance = dateNaissance;
        }
    }
}