using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Models;

using Microsoft.EntityFrameworkCore;

namespace ClinicConnectApi.Services
{
    public class PatientsService : IPatientsService
    {
        private readonly AppDbContext _db;
        public PatientsService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync()
        {
            return await _db.Patients
            .Select(p => new PatientResponseDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Pesel = p.Pesel
            }).ToListAsync();
        }
        public async Task<PatientResponseDto> CreatePatientAsync(CreatePatientDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) || string.IsNullOrWhiteSpace(dto.Pesel))
            {
                throw new ArgumentException("Incorrect input");
            }
            var existingPatient = await _db.Patients.FirstOrDefaultAsync(p => p.Pesel == dto.Pesel);
            if (existingPatient != null)
            {
                throw new InvalidOperationException("Patient already exists");
            }
            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Pesel = dto.Pesel
            };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            return new PatientResponseDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Pesel = patient.Pesel
            };
        }
        public async Task DeletePatientAsync(int id)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient == null)
            {
                throw new KeyNotFoundException("Patient doesn't exist");
            }
            _db.Remove(patient);
            await _db.SaveChangesAsync();
        }
    }
}