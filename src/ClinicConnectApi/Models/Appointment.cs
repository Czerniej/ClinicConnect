namespace ClinicConnectApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}