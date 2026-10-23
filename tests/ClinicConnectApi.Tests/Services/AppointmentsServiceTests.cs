using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
using ClinicConnectApi.Models;
using ClinicConnectApi.Services;
using ClinicConnectApi.Tests.Infrastructure;

using Microsoft.Extensions.Time.Testing;

namespace ClinicConnectApi.Tests.Services
{
    public class AppointmentsServiceTests : IDisposable
    {
        private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        private readonly SqliteTestDatabase _database = new();
        private readonly FakeTimeProvider _time = new(Now);
        private readonly AppDbContext _db;
        private readonly AppointmentsService _service;
        public AppointmentsServiceTests()
        {
            _db = _database.CreateContext();
            _service = new AppointmentsService(_db, _time);
        }
        public void Dispose()
        {
            _db.Dispose();
            _database.Dispose();
        }
        private async Task<(Doctor Doctor, Patient Patient)> SeedAsync()
        {
            var doctor = new Doctor
            {
                FirstName = "Till",
                LastName = "Lindemann",
                Specialization = "Cardiology",
                ConsultationFee = 200m
            };
            var patient = new Patient
            {
                FirstName = "Nico",
                LastName = "Sallach",
                Pesel = "44051401359"
            };
            _db.Doctors.Add(doctor);
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            return (doctor, patient);
        }
        private static CreateAppointmentDto Dto(int doctorId, int patientId, DateTime date) => new()
        {
            DoctorId = doctorId,
            PatientId = patientId,
            AppointmentDate = date
        };
        [Fact]
        public async Task GetAllAppointmentsAsync_ReturnsAppointmentsOrderedByDate()
        {
            var (doctor, patient) = await SeedAsync();
            var late = new DateTime(2026, 1, 3, 12, 0, 0, DateTimeKind.Utc);
            var early = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
            await _service.CreateAppointmentAsync(Dto(doctor.Id, patient.Id, late));
            await _service.CreateAppointmentAsync(Dto(doctor.Id, patient.Id, early));

            var result = await _service.GetAllAppointmentsAsync();

            Assert.Equal(new[] { early, late }, result.Select(a => a.AppointmentDate));
        }
        [Fact]
        public async Task CreateAppointmentAsync_ValidData_ReturnsAppointment()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);

            var result = await _service.CreateAppointmentAsync(dto);

            Assert.Equal("Till Lindemann", result.DoctorName);
            Assert.Equal("Nico Sallach", result.PatientName);
            Assert.Equal(dto.AppointmentDate, result.AppointmentDate);
            Assert.False(result.IsCompleted);
        }
        [Fact]
        public async Task CreateAppointmentAsync_DateInThePast_ThrowsBadRequest()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAppointmentAsync(dto));
        }
        [Fact]
        public async Task CreateAppointmentAsync_OutOfGrid_ThrowsBadRequest()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 1, 5, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);

            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateAppointmentAsync(dto));
        }
        [Fact]
        public async Task CreateAppointmentAsync_NonExistingDoctor_ThrowsNotFound()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);
            var dto = Dto(999, patient.Id, appointmentDate);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAppointmentAsync(dto));
        }
        [Fact]
        public async Task CreateAppointmentAsync_NonExistingPatient_ThrowsNotFound()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, 999, appointmentDate);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAppointmentAsync(dto));
        }
        [Fact]
        public async Task CreateAppointmentAsync_WhenDoctorIsBusy_ThrowsConflict()
        {
            var (doctor, patient1) = await SeedAsync();
            var patient2 = new Patient
            {
                FirstName = "Kyle",
                LastName = "Crane",
                Pesel = "93110604750"
            };
            _db.Patients.Add(patient2);
            await _db.SaveChangesAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);

            var dto1 = Dto(doctor.Id, patient1.Id, appointmentDate);
            var dto2 = Dto(doctor.Id, patient2.Id, appointmentDate);

            await _service.CreateAppointmentAsync(dto1);

            await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAppointmentAsync(dto2));
        }
        [Fact]
        public async Task CreateAppointmentAsync_WhenPatientIsBusy_ThrowsConflict()
        {
            var (doctor1, patient) = await SeedAsync();
            var doctor2 = new Doctor
            {
                FirstName = "Cal",
                LastName = "Cestis",
                Specialization = "Radiologist",
                ConsultationFee = 200m
            };
            _db.Doctors.Add(doctor2);
            await _db.SaveChangesAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);

            var dto1 = Dto(doctor1.Id, patient.Id, appointmentDate);
            var dto2 = Dto(doctor2.Id, patient.Id, appointmentDate);

            await _service.CreateAppointmentAsync(dto1);
            await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAppointmentAsync(dto2));
        }
        [Fact]
        public async Task CreateAppointmentAsync_SameTimeDifferentDoctorAndPatient_BothSucceed()
        {
            var (doctor1, patient1) = await SeedAsync();
            var doctor2 = new Doctor
            {
                FirstName = "Cal",
                LastName = "Cestis",
                Specialization = "Radiologist",
                ConsultationFee = 200m
            };
            var patient2 = new Patient
            {
                FirstName = "Kyle",
                LastName = "Crane",
                Pesel = "93110604750"
            };
            _db.Doctors.Add(doctor2);
            _db.Patients.Add(patient2);
            await _db.SaveChangesAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);

            var dto1 = Dto(doctor1.Id, patient1.Id, appointmentDate);
            var dto2 = Dto(doctor2.Id, patient2.Id, appointmentDate);

            var result1 = await _service.CreateAppointmentAsync(dto1);
            var result2 = await _service.CreateAppointmentAsync(dto2);

            Assert.NotNull(result1);
            Assert.Equal("Till Lindemann", result1.DoctorName);
            Assert.Equal("Nico Sallach", result1.PatientName);
            Assert.NotNull(result2);
            Assert.Equal("Cal Cestis", result2.DoctorName);
            Assert.Equal("Kyle Crane", result2.PatientName);
        }
        [Fact]
        public async Task CreateAppointmentAsync_UnspecifiedDateKind_TreatsDateAsUtc()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Unspecified);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);

            var result = await _service.CreateAppointmentAsync(dto);

            Assert.Equal(new DateTime(2026, 10, 20, 16, 0, 0), result.AppointmentDate);
            Assert.Equal(DateTimeKind.Utc, result.AppointmentDate.Kind);
        }
        [Fact]
        public async Task CompleteAppointmentAsync_BeforeAppointmentDate_ThrowsConflict()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Unspecified);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);
            var appointment = await _service.CreateAppointmentAsync(dto);

            await Assert.ThrowsAsync<ConflictException>(() => _service.CompleteAppointmentAsync(appointment.Id, new CompleteAppointmentDto
            {
                Notes = "Patient is healthy"
            }));
        }
        [Fact]
        public async Task CompleteAppointmentAsync_AfterAppointmentDate_MarksAsCompleted()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 1, 1, 16, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);
            var appointment = await _service.CreateAppointmentAsync(dto);
            _time.Advance(TimeSpan.FromDays(2));

            var result = await _service.CompleteAppointmentAsync(appointment.Id, new CompleteAppointmentDto
            {
                Notes = "Patient is healthy"
            });

            Assert.True(result.IsCompleted);
            Assert.Equal("Patient is healthy", result.Notes);
        }
        [Fact]
        public async Task CompleteAppointmentAsync_AlreadyCompleted_ThrowsConflict()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 1, 1, 16, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);
            var appointment = await _service.CreateAppointmentAsync(dto);
            _time.Advance(TimeSpan.FromDays(2));
            await _service.CompleteAppointmentAsync(appointment.Id, new CompleteAppointmentDto
            {
                Notes = "Patient is healthy"
            });

            await Assert.ThrowsAsync<ConflictException>(() => _service.CompleteAppointmentAsync(appointment.Id, new CompleteAppointmentDto
            {
                Notes = "Patient is healthy"
            }));
        }
        [Fact]
        public async Task CompleteAppointmentAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CompleteAppointmentAsync(999, new CompleteAppointmentDto
            {
                Notes = "Patient is healthy"
            }));
        }
        [Fact]
        public async Task GetAppointmentByIdAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAppointmentByIdAsync(999));
        }
        [Fact]
        public async Task GetAppointmentByIdAsync_ExistingId_ReturnsAppointment()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);
            var appointment = await _service.CreateAppointmentAsync(dto);

            Assert.Equal("Till Lindemann", appointment.DoctorName);
            Assert.Equal("Nico Sallach", appointment.PatientName);
            Assert.Equal(dto.AppointmentDate, appointment.AppointmentDate);
            Assert.False(appointment.IsCompleted);
        }
        [Fact]
        public async Task DeleteAppointmentAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAppointmentAsync(999));
        }
        [Fact]
        public async Task DeleteAppointmentAsync_ExistingId_RemovesAppointment()
        {
            var (doctor, patient) = await SeedAsync();
            var appointmentDate = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc);
            var dto = Dto(doctor.Id, patient.Id, appointmentDate);
            var appointment = await _service.CreateAppointmentAsync(dto);

            await _service.DeleteAppointmentAsync(appointment.Id);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAppointmentByIdAsync(appointment.Id));
        }
    }
}