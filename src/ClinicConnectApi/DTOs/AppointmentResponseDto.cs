namespace ClinicConnectApi.Dtos
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}