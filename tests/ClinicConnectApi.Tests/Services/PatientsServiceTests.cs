using ClinicConnectApi.Data;
using ClinicConnectApi.Dtos;
using ClinicConnectApi.Exceptions;
using ClinicConnectApi.Models;
using ClinicConnectApi.Services;
using ClinicConnectApi.Tests.Infrastructure;

namespace ClinicConnectApi.Tests.Services
{
    public class PatientsServiceTests : IDisposable
    {
        private readonly SqliteTestDatabase _database = new();
        private readonly AppDbContext _db;
        private readonly PatientsService _service;
        public PatientsServiceTests()
        {
            _db = _database.CreateContext();
            _service = new PatientsService(_db);
        }
        public void Dispose()
        {
            _db.Dispose();
            _database.Dispose();
        }
        private async Task<(Patient First, Patient Second)> SeedAsync()
        {
            var patient1 = new Patient
            {
                FirstName = "Jon",
                LastName = "Snow",
                Pesel = "06220719438"
            };
            var patient2 = new Patient
            {
                FirstName = "Robert",
                LastName = "Arryn",
                Pesel = "06240216098"
            };
            _db.Patients.Add(patient1);
            _db.Patients.Add(patient2);
            await _db.SaveChangesAsync();
            return (patient1, patient2);
        }
        private static CreatePatientDto Dto(string firstName = "Brandon", string lastName = "Stark", string pesel = "01320215673") => new()
        {
            FirstName = firstName,
            LastName = lastName,
            Pesel = pesel
        };
        [Fact]
        public async Task GetAllPatientsAsync_ReturnsPatientsOrderedByLastName()
        {
            var (patient1, patient2) = await SeedAsync();
        
            var result = await _service.GetAllPatientsAsync();
        
            Assert.Equal(new[] { patient2.LastName, patient1.LastName }, result.Select(p => p.LastName));
        }
        [Fact]
        public async Task GetPatientByIdAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPatientByIdAsync(999));
        }
        [Fact]
        public async Task GetPatientByIdAsync_ExistingId_ReturnsPatient()
        {
            var (_, patient) = await SeedAsync();

            var result = await _service.GetPatientByIdAsync(patient.Id);

            Assert.Equal("Robert", result.FirstName);
            Assert.Equal("Arryn", result.LastName);
            Assert.Equal("06240216098", result.Pesel);
        }
        [Fact]
        public async Task CreatePatientAsync_SamePesel_ThrowsConflict()
        {
            var dto1 = Dto();
            var dto2 = Dto("Sansa", "Stark", "01320215673");
            
            await _service.CreatePatientAsync(dto1);
        
            await Assert.ThrowsAsync<ConflictException>(() => _service.CreatePatientAsync(dto2));
        }
        [Fact]
        public async Task CreatePatientAsync_DifferentPeople_BothSucceed()
        {
            var dto1 = Dto();
            var dto2 = Dto("Oberyn", "Martell", "01312014514");
        
            var result1 = await _service.CreatePatientAsync(dto1);
            var result2 = await _service.CreatePatientAsync(dto2);
        
            Assert.Equal("Brandon", result1.FirstName);
            Assert.Equal("Stark", result1.LastName);
            Assert.Equal("01320215673", result1.Pesel);
            Assert.True(result1.Id > 0);
            Assert.Equal("Oberyn", result2.FirstName);
            Assert.Equal("Martell", result2.LastName);
            Assert.Equal("01312014514", result2.Pesel);
            Assert.True(result2.Id > 0);
            Assert.NotEqual(result1.Id, result2.Id);
        }
        [Fact]
        public async Task DeletePatientAsync_NonExistingId_ThrowsNotFound()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeletePatientAsync(999));
        }
        [Fact]
        public async Task DeletePatientAsync_ExistingId_RemovesPatient()
        {
            var (patient, _) = await SeedAsync();

            await _service.DeletePatientAsync(patient.Id);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPatientByIdAsync(patient.Id));
        }
        [Fact]
        public async Task DeletePatientAsync_PatientHasAppointment_ThrowsConflict()
        {
            var (_, patient) = await SeedAsync();
            var doctor = new Doctor
            {
                FirstName = "Till",
                LastName = "Lindemann",
                Specialization = "Cardiology",
                ConsultationFee = 200m
            };
            _db.Doctors.Add(doctor);
            await _db.SaveChangesAsync();
            _db.Appointments.Add(new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                AppointmentDate = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            });
            await _db.SaveChangesAsync();
        
            await Assert.ThrowsAsync<ConflictException>(() => _service.DeletePatientAsync(patient.Id));
            var stillThere = await _service.GetPatientByIdAsync(patient.Id);
            Assert.Equal(patient.Id, stillThere.Id);
        }
    }
}