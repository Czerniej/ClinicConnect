using ClinicConnectApi.Dtos;

namespace ClinicConnectApi.Services
{
    public interface IDoctorsService
    {
        Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();
        Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto);
        Task DeleteDoctorAsync(int id);
    }
}