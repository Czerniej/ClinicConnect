using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
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
            var existingPatient = await _db.Patients.FirstOrDefaultAsync(p => p.Pesel == dto.Pesel);
            if (existingPatient != null)
            {
                throw new ConflictException("Patient with this pesel already exists");
            }
            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Pesel = dto.Pesel
            };
            _db.Patients.Add(patient);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch(DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new ConflictException("Patient with this pesel already exists");
            }
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
            var patient = await _db.Patients.FindAsync(id)
                ?? throw new NotFoundException($"Patient with id: {id} was not found");
            bool hasAppointments = await _db.Appointments.AnyAsync(a => a.PatientId == id);
            if (hasAppointments)
            {
                throw new ConflictException("Patient has appointments and can't be deleted");
            }
            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
        }
    }
}