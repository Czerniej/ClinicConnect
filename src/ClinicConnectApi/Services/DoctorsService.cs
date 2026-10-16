using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
using ClinicConnectApi.Mappings;
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
                .OrderBy(d => d.LastName)
                .Select(DoctorMappings.ToResponseDto)
                .ToListAsync();
        }
        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto)
        {
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
            var doctor = await _db.Doctors.FindAsync(id)
                ?? throw new NotFoundException($"Doctor with id: {id} was not found");
            bool hasAppointments = await _db.Appointments.AnyAsync(a => a.DoctorId == id);
            if (hasAppointments)
            {
                throw new ConflictException("Doctor has appointments and can't be deleted");
            }
            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync();
        }
    }
}