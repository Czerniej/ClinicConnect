using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
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
            .Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                DoctorName = $"{a.Doctor.FirstName} {a.Doctor.LastName}",
                PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                AppointmentDate = a.AppointmentDate,
                IsCompleted = a.IsCompleted,
                Notes = a.Notes
            }).ToListAsync();
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
            await _db.SaveChangesAsync();
            return new AppointmentResponseDto
            {
                Id = appointment.Id,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                PatientName = $"{patient.FirstName} {patient.LastName}",
                AppointmentDate = appointment.AppointmentDate,
                IsCompleted = appointment.IsCompleted,
                Notes = appointment.Notes
            };
        }
        public async Task DeleteAppointmentAsync(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
            {
                throw new NotFoundException($"Appointment with id: {id} was not found");
            }
            _db.Appointments.Remove(appointment);
            await _db.SaveChangesAsync();
        }
    }
}