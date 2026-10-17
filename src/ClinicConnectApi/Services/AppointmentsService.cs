using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
using ClinicConnectApi.Mappings;
using ClinicConnectApi.Models;

using Microsoft.EntityFrameworkCore;

namespace ClinicConnectApi.Services
{
    public class AppointmentsService : IAppointmentsService
    {
        private readonly AppDbContext _db;
        public AppointmentsService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync()
        {
            return await _db.Appointments
                .OrderBy(a => a.AppointmentDate)
                .Select(AppointmentMappings.ToResponseDto)
                .ToListAsync();
        }
        public async Task<AppointmentResponseDto> GetAppointmentById(int id)
        {
            return await _db.Appointments
                .Where(a => a.Id == id)
                .Select(AppointmentMappings.ToResponseDto)
                .FirstOrDefaultAsync()
                    ?? throw new NotFoundException($"Appointment with id: {id} was not found");
        }
        public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto)
        {
            var doctor = await _db.Doctors.FindAsync(dto.DoctorId)
                ?? throw new NotFoundException($"Doctor with id: {dto.DoctorId} wos not found");
            var patient = await _db.Patients.FindAsync(dto.PatientId)
                ?? throw new NotFoundException($"Patient with id: {dto.PatientId} was not found");
            bool isBusyDoctor = await _db.Appointments.AnyAsync(a => a.DoctorId == dto.DoctorId && a.AppointmentDate == dto.AppointmentDate);
            if (isBusyDoctor)
            {
                throw new NotFoundException("Doctor already has appointment at this date");
            }
            bool isBusyPatient = await _db.Appointments.AnyAsync(a => a.PatientId == dto.PatientId && a.AppointmentDate == dto.AppointmentDate);
            if (isBusyPatient)
            {
                throw new NotFoundException("Patient already has appointment at this date");
            }
            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                AppointmentDate = dto.AppointmentDate,
                IsCompleted = false,
                Notes = ""
            };
            _db.Appointments.Add(appointment);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new ConflictException("Doctor or patient already has an appointment at this date");
            }
            return await GetAppointmentById(appointment.Id);
        }
        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id)
                ?? throw new NotFoundException($"Appointment with id: {id} was not found");
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
        }
    }
}