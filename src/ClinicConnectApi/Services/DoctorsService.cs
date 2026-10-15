using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Models;

using Microsoft.EntityFrameworkCore;

namespace ClinicConnectApi.Services
{
    public class DoctorsService : IDoctorsService
    {
        private readonly AppDbContext _db;
        public DoctorsService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync()
        {
            return await _db.Doctors
            .Select(d => new DoctorResponseDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Specialization = d.Specialization,
                ConsultationFee = d.ConsultationFee
            }).ToListAsync();
        }
        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName)
             || string.IsNullOrWhiteSpace(dto.Specialization) || dto.ConsultationFee <= 0)
            {
                throw new ArgumentException("Incorrect input");
            }
            var doctor = new Doctor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Specialization = dto.Specialization,
                ConsultationFee = dto.ConsultationFee
            };
            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();
            return new DoctorResponseDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialization = doctor.Specialization,
                ConsultationFee = doctor.ConsultationFee
            };
        }
        public async Task DeleteDoctorAsync(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor doesn't exist");
            }
            bool hasAppointments = await _db.Appointments.AnyAsync(a => a.DoctorId == id);
            if (hasAppointments)
            {
                throw new InvalidOperationException("Doctor has appointments and can't be deleted");
            }
            _db.Remove(doctor);
            await _db.SaveChangesAsync();
        }
    }
}