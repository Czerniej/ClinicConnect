using System.ComponentModel.DataAnnotations;

using ClinicConnectApi.Validation;

namespace ClinicConnectApi.Dtos
{
    public class CreatePatientDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [Required, Pesel]
        public string Pesel { get; set; } = string.Empty;
    }
}