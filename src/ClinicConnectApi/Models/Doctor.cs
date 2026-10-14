namespace ClinicConnectApi.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public decimal ConsultationFee { get; set; }
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}