using ClinicConnectApi.Dtos;
using ClinicConnectApi.Services;

using Microsoft.AspNetCore.Mvc;

namespace ClinicConnectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsService _appointmentsService;
        public AppointmentsController(IAppointmentsService appointmentsService)
        {
            _appointmentsService = appointmentsService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAll()
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }
        [HttpPost]
        public async Task<ActionResult<AppointmentResponseDto>> Create(CreateAppointmentDto dto)
        {
            var newAppointment = await _appointmentsService.CreateAppointmentAsync(dto);
            return StatusCode(StatusCodes.Status201Created, newAppointment);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _appointmentsService.DeleteAppointmentAsync(id);
            return NoContent();
            
        }
    }
}