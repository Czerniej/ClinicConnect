using ClinicConnectApi.Dtos;
using ClinicConnectApi.Services;

using Microsoft.AspNetCore.Mvc;

namespace ClinicConnectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PatientsController : ControllerBase
    {
        private readonly IPatientsService _patientsService;
        public PatientsController(IPatientsService patientsService)
        {
            _patientsService = patientsService;
        }
        [HttpGet]
        public async Task<IResult> GetAll()
        {
            var patients = await _patientsService.GetAllPatientsAsync();
            return Results.Ok(patients);
        }
        [HttpPost]
        public async Task<IResult> Create(CreatePatientDto dto)
        {
            try
            {
                var newPatient = await _patientsService.CreatePatientAsync(dto);
                return Results.Ok(newPatient);
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
                await _patientsService.DeletePatientAsync(id);
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