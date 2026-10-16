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
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PatientResponseDto>> GetById(int id)
        {
            var patient = await _patientsService.GetPatientByIdAsync(id);
            return Ok(patient);
        }
        [HttpPost]
        public async Task<ActionResult<PatientResponseDto>> Create(CreatePatientDto dto)
        {
            var newPatient = await _patientsService.CreatePatientAsync(dto);
            return CreatedAtAction(nameof(GetById), new {id = newPatient.Id}, newPatient);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _patientsService.DeletePatientAsync(id);
            return NoContent();
        }
    }
}