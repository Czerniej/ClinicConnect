namespace ClinicConnectApi.Dtos
{
    public class CreateAppointmentDto
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}