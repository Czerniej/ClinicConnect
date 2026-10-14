namespace ClinicConnectApi.Dtos
{
    public class CreateDoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}