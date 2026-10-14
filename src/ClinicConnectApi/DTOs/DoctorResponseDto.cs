namespace ClinicConnectApi.Dtos
{
    public class DoctorResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public decimal ConsultationFee { get; set; }
    }
}