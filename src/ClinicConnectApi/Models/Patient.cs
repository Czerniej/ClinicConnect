namespace ClinicConnectApi.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pesel { get; set; }
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}