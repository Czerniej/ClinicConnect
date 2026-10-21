using System.ComponentModel.DataAnnotations;

namespace ClinicConnectApi.Dtos
{
    public class CreateDoctorDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Specialization { get; set; } = string.Empty;
        [Range(0, double.MaxValue, MinimumIsExclusive = true)]
        public decimal ConsultationFee { get; set; }
    }
}