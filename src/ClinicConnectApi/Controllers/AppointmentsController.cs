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
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetById(int id)
        {
            var appointment = await _appointmentsService.GetAppointmentById(id);
            return Ok(appointment);
        }
        [HttpPost]
        public async Task<ActionResult<AppointmentResponseDto>> Create(CreateAppointmentDto dto)
        {
            var newAppointment = await _appointmentsService.CreateAppointmentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newAppointment.Id }, newAppointment);
        }
        [HttpPatch("{id:int}/complete")]
        public async Task<ActionResult<AppointmentResponseDto>> Complete(int id, CompleteAppointmentDto dto)
        {
            var completed = await _appointmentsService.CompleteAppointmentAsync(id, dto);
            return Ok(completed);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _appointmentsService.DeleteAppointmentAsync(id);
            return NoContent();
            
        }
    }
}