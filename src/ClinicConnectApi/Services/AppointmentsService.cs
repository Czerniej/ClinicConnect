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
        private readonly TimeProvider _timeProvider;
        private static readonly TimeSpan SlotLength = TimeSpan.FromMinutes(30);
        public AppointmentsService(AppDbContext db, TimeProvider timeProvider)
        {
            _db = db;
            _timeProvider = timeProvider;
        }
        public async Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync()
        {
            return await _db.Appointments
                .OrderBy(a => a.AppointmentDate)
                .Select(AppointmentMappings.ToResponseDto)
                .ToListAsync();
        }
        public async Task<AppointmentResponseDto> GetAppointmentByIdAsync(int id)
        {
            return await _db.Appointments
                .Where(a => a.Id == id)
                .Select(AppointmentMappings.ToResponseDto)
                .FirstOrDefaultAsync()
                    ?? throw new NotFoundException($"Appointment with id: {id} was not found");
        }
        public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto)
        {
            var appointmentDate = ToUtc(dto.AppointmentDate);
            if(appointmentDate.Ticks % SlotLength.Ticks != 0)
            {
                throw new BadRequestException("Appointments must start at full or half hour");
            }
            if(appointmentDate <= _timeProvider.GetUtcNow().UtcDateTime)
            {
                throw new BadRequestException("Appointment date must be in the future");
            }
            var doctor = await _db.Doctors.FindAsync(dto.DoctorId)
                ?? throw new NotFoundException($"Doctor with id: {dto.DoctorId} was not found");
            var patient = await _db.Patients.FindAsync(dto.PatientId)
                ?? throw new NotFoundException($"Patient with id: {dto.PatientId} was not found");
            bool isBusyDoctor = await _db.Appointments.AnyAsync(a => a.DoctorId == dto.DoctorId && a.AppointmentDate == appointmentDate);
            if (isBusyDoctor)
            {
                throw new ConflictException("Doctor already has appointment at this date");
            }
            bool isBusyPatient = await _db.Appointments.AnyAsync(a => a.PatientId == dto.PatientId && a.AppointmentDate == appointmentDate);
            if (isBusyPatient)
            {
                throw new ConflictException("Patient already has appointment at this date");
            }
            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                AppointmentDate = appointmentDate,
                IsCompleted = false,
                Notes = string.Empty
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
            return await GetAppointmentByIdAsync(appointment.Id);
        }
        public async Task<AppointmentResponseDto> CompleteAppointmentAsync(int id, CompleteAppointmentDto dto)
        {
            var appointment = await _db.Appointments.FindAsync(id)
                ?? throw new NotFoundException($"Appointment with id {id} was not found");
            if (appointment.IsCompleted)
            {
                throw new ConflictException("Appointment is already completed");
            }
            if (appointment.AppointmentDate > _timeProvider.GetUtcNow().UtcDateTime)
            {
                throw new ConflictException("An appointment that hasn't started yet can't be completed");
            }
            appointment.IsCompleted = true;
            appointment.Notes = dto.Notes;
            await _db.SaveChangesAsync();
            return await GetAppointmentByIdAsync(id);
        }
        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id)
                ?? throw new NotFoundException($"Appointment with id: {id} was not found");
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
        }
        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}