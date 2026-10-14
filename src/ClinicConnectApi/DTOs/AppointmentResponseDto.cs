namespace ClinicConnectApi.Dtos
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; }
    }
}