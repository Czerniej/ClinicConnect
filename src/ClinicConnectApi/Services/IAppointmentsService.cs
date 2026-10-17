using ClinicConnectApi.Dtos;

namespace ClinicConnectApi.Services
{
    public interface IAppointmentsService
    {
        Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync();
        Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id);
        Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto);
        Task<AppointmentResponseDto> CompleteAppointmentAsync(int id, CompleteAppointmentDto dto);
        Task DeleteAppointmentAsync(int id);
    }
}