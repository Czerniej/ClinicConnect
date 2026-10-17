using System.ComponentModel.DataAnnotations;

namespace ClinicConnectApi.Dtos
{
    public class CompleteAppointmentDto
    {
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }
}