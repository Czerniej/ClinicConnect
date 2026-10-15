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
        public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetAll()
        {
            var patients = await _patientsService.GetAllPatientsAsync();
            return Ok(patients);
        }
        [HttpPost]
        public async Task<ActionResult<PatientResponseDto>> Create(CreatePatientDto dto)
        {
            var newPatient = await _patientsService.CreatePatientAsync(dto);
            return StatusCode(StatusCodes.Status201Created, newPatient);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _patientsService.DeletePatientAsync(id);
            return NoContent();
        }
    }
}