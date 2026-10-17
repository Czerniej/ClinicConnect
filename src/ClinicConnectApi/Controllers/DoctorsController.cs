using System.Runtime.CompilerServices;

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
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetAll()
        {
            var doctors = await _doctorsService.GetAllDoctorsAsync();
            return Ok(doctors);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DoctorResponseDto>> GetById(int id)
        {
            var doctor = await _doctorsService.GetDoctorByIdAsync(id);
            return Ok(doctor);
        }
        [HttpPost]
        public async Task<ActionResult<DoctorResponseDto>> Create(CreateDoctorDto dto)
        {
            var newDoctor = await _doctorsService.CreateDoctorAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newDoctor.Id }, newDoctor);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _doctorsService.DeleteDoctorAsync(id);
            return NoContent();
        }
    }
}