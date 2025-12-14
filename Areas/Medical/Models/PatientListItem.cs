using System.Text.Json.Serialization;

namespace CabinetMedicalWeb.Areas.Medical.Models
{
    public record PatientListItem(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("fullName")] string FullName,
        [property: JsonPropertyName("telephone")] string Telephone,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("dateNaissance")] string DateNaissance
    );
}
