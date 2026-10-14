using ClinicConnectApi.Dtos;

namespace ClinicConnectApi.Services
{
    public interface IPatientsService
    {
        Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync();
        Task<PatientResponseDto> CreatePatientAsync(CreatePatientDto dto);
        Task DeletePatientAsync(int id);
    }
}