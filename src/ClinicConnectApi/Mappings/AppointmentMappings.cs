using System.Linq.Expressions;

using ClinicConnectApi.Dtos;
using ClinicConnectApi.Models;

namespace ClinicConnectApi.Mappings
{
    public static class AppointmentMappings
    {
        public static readonly Expression<Func<Appointment, AppointmentResponseDto>> ToResponseDto =
            a => new AppointmentResponseDto
            {
                Id = a.Id,
                DoctorName = $"{a.Doctor.FirstName} {a.Doctor.LastName}",
                PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                AppointmentDate = a.AppointmentDate,
                IsCompleted = a.IsCompleted,
                Notes = a.Notes
            };
    }
}