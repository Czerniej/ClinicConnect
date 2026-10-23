using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
using ClinicConnectApi.Models;
using ClinicConnectApi.Services;
using ClinicConnectApi.Tests.Infrastructure;

namespace ClinicConnectApi.Tests.Services
{
    public class DoctorsServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _database = new();
        private readonly AppDbContext _db;
        private readonly DoctorsService _service;
        public DoctorsServiceTests()
        {
            _db = _database.CreateContext();
            _service = new DoctorsService(_db);
        }
        public void Dispose()
        {
            _db.Dispose();
            _database.Dispose();
        }
        private async Task<(Doctor First, Doctor Second)> SeedAsync()
        {
            var doctor1 = new Doctor
            {
                FirstName = "James",
                LastName = "Wilton",
                Specialization = "Oncologist",
                ConsultationFee = 200m
            };
            var doctor2 = new Doctor
            {
                FirstName = "Gregory",
                LastName = "House",
                Specialization = "Pathologist",
                ConsultationFee = 200m
            };
            _db.Doctors.Add(doctor1);
            _db.Doctors.Add(doctor2);
            await _db.SaveChangesAsync();
            return (doctor1, doctor2);
        }
        private static CreateDoctorDto Dto(string firstName = "Lisa", string lastName = "Cuddy", string specialization = "Endocrinologist", decimal consultationFee = 200m) => new()
        {
            FirstName = firstName,
            LastName = lastName,
            Specialization = specialization,
            ConsultationFee = consultationFee
        };
        [Fact]
        public async Task GetAllDoctorsAsync_ReturnsDoctorsOrderedByLastName()
        {
            var (doctor1, doctor2) = await SeedAsync();
            var result = await _service.GetAllDoctorsAsync();
            Assert.Equal(new[] { doctor2.LastName, doctor1.LastName }, result.Select(d => d.LastName));
        }
        [Fact]
        public async Task GetDoctorByIdAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetDoctorByIdAsync(999));
        }
        [Fact]
        public async Task GetDoctorByIdAsync_ExistingId_ReturnsDoctor()
        {
            var (_, doctor) = await SeedAsync();

            var result = await _service.GetDoctorByIdAsync(doctor.Id);

            Assert.Equal(doctor.FirstName, result.FirstName);
            Assert.Equal(doctor.LastName, result.LastName);
            Assert.Equal(doctor.Specialization, result.Specialization);
            Assert.Equal(doctor.ConsultationFee, result.ConsultationFee);
        }
        [Fact]
        public async Task CreateDoctorAsync_DifferentPeople_BothSucceed()
        {
            var dto1 = Dto();
            var dto2 = Dto("Eric", "Foreman", "Neurologist", 200m);

            var result1 = await _service.CreateDoctorAsync(dto1);
            var result2 = await _service.CreateDoctorAsync(dto2);

            Assert.Equal(dto1.FirstName, result1.FirstName);
            Assert.Equal(dto1.LastName, result1.LastName);
            Assert.Equal(dto1.Specialization, result1.Specialization);
            Assert.Equal(dto1.ConsultationFee, result1.ConsultationFee);
            Assert.True(result1.Id > 0);
            Assert.Equal(dto2.FirstName, result2.FirstName);
            Assert.Equal(dto2.LastName, result2.LastName);
            Assert.Equal(dto2.Specialization, result2.Specialization);
            Assert.Equal(dto2.ConsultationFee, result2.ConsultationFee);
            Assert.True(result2.Id > 0);
            Assert.NotEqual(result1.Id, result2.Id);
        }
        [Fact]
        public async Task DeleteDoctorAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteDoctorAsync(999));
        }
        [Fact]
        public async Task DeleteDoctorAsync_ExistingId_RemovesDoctor()
        {
            var (doctor, _) = await SeedAsync();

            await _service.DeleteDoctorAsync(doctor.Id);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetDoctorByIdAsync(doctor.Id));
        }
        [Fact]
        public async Task DeleteDoctorAsync_DoctorHasAppointment_ThrowsConflict()
        {
            var (doctor, _) = await SeedAsync();
            var patient = new Patient
            {
                FirstName = "Oberyn",
                LastName = "Martell",
                Pesel = "01312014514"
            };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            _db.Appointments.Add(new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                AppointmentDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            });
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteDoctorAsync(doctor.Id));
            var stillThere = await _service.GetDoctorByIdAsync(doctor.Id);
            Assert.Equal(doctor.Id, stillThere.Id);
        }
    }
}