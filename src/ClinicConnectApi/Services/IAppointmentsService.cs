using ClinicConnectApi.Dtos;

namespace ClinicConnectApi.Services
{
    public interface IAppointmentsService
    {
        Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync();
        Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto);
        Task DeleteAppointmentAsync(int id);
    }
}