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
        public async Task<IResult> GetAll()
        {
            var appointments = await _appointmentsService.GetAllAppointmentsAsync();
            return Results.Ok(appointments);
        }
        [HttpPost]
        public async Task<IResult> Create(CreateAppointmentDto dto)
        {
            try
            {
                var newAppointment = await _appointmentsService.CreateAppointmentAsync(dto);
                return Results.Ok(newAppointment);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
            catch (Exception)
            {
                return Results.Problem("Server Error");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IResult> Delete(int id)
        {
            try
            {
                await _appointmentsService.DeleteAppointmentAsync(id);
                return Results.Ok("Successfully deleted");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
            catch (Exception)
            {
                return Results.Problem("Server Error");
            }
        }
    }
}