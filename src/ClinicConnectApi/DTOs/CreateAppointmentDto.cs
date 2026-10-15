using System.ComponentModel.DataAnnotations;

namespace ClinicConnectApi.Dtos
{
    public class CreateAppointmentDto
    {
        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }
        [Range(1, int.MaxValue)]
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}