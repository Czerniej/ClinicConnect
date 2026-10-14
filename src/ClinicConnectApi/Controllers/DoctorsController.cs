using ClinicConnectApi.Dtos;
using ClinicConnectApi.Services;

using Microsoft.AspNetCore.Mvc;

namespace ClinicConnectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorsService _doctorsService;
        public DoctorsController(IDoctorsService doctorsService)
        {
            _doctorsService = doctorsService;
        }
        [HttpGet]
        public async Task<IResult> GetAll()
        {
            var doctors = await _doctorsService.GetAllDoctorsAsync();
            return Results.Ok(doctors);
        }
        [HttpPost]
        public async Task<IResult> Create(CreateDoctorDto dto)
        {
            try
            {
                var newDoctor = await _doctorsService.CreateDoctorAsync(dto);
                return Results.Ok(newDoctor);
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
                await _doctorsService.DeleteDoctorAsync(id);
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