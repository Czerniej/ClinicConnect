using System.Net;
using System.Net.Http.Json;

using ClinicConnectApi.Dtos;
using ClinicConnectApi.Tests.Infrastructure;

namespace ClinicConnectApi.Tests.Api
{
    public class AppointmentsApiTests : ApiTestBase
    {
        private async Task<HttpResponseMessage> PostAppointmentAsync(int doctorId, int patientId, DateTime date)
            => await Client.PostAsJsonAsync("/api/appointments", new CreateAppointmentDto
            {
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = date
            });

        [Fact]
        public async Task Create_ValidAppointment_Returns201WithUtcDateAndLocation()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();

            var response = await PostAppointmentAsync(doctor.Id, patient.Id, FutureSlot);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await ReadJsonAsync(response);
            var id = body.GetProperty("id").GetInt32();
            Assert.EndsWith($"/api/Appointments/{id}",
                response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.Equal("Gregory House", body.GetProperty("doctorName").GetString());
            Assert.Equal("Brandon Stark", body.GetProperty("patientName").GetString());
            Assert.EndsWith("Z", body.GetProperty("appointmentDate").GetString());
        }

        [Fact]
        public async Task Create_SameSlotTwice_Returns409()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();

            var first = await PostAppointmentAsync(doctor.Id, patient.Id, FutureSlot);
            var second = await PostAppointmentAsync(doctor.Id, patient.Id, FutureSlot);

            Assert.Equal(HttpStatusCode.Created, first.StatusCode);
            await AssertProblemAsync(second, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Create_DateInThePast_Returns400()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            var past = new DateTime(2025, 12, 31, 10, 0, 0, DateTimeKind.Utc);

            var response = await PostAppointmentAsync(doctor.Id, patient.Id, past);

            await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_DateNotOnSlotGrid_Returns400()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            var offGrid = new DateTime(2026, 1, 2, 10, 15, 0, DateTimeKind.Utc);

            var response = await PostAppointmentAsync(doctor.Id, patient.Id, offGrid);

            await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_MissingDate_Returns400()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();

            var response = await Client.PostAsJsonAsync("/api/appointments",
                new { doctorId = doctor.Id, patientId = patient.Id });

            await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_NonExistingDoctor_Returns404()
        {
            var patient = await CreatePatientAsync();

            var response = await PostAppointmentAsync(999, patient.Id, FutureSlot);

            await AssertProblemAsync(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Complete_BeforeAppointmentStart_Returns409()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            var appointment = await CreateAppointmentAsync(doctor.Id, patient.Id);

            var response = await PatchJsonAsync(
                $"/api/appointments/{appointment.Id}/complete",
                new CompleteAppointmentDto { Notes = "Too early" });

            await AssertProblemAsync(response, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Complete_AfterAppointmentStart_Returns200AndStoresNotes()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            var appointment = await CreateAppointmentAsync(doctor.Id, patient.Id);
            Factory.Time.Advance(TimeSpan.FromDays(2));

            var response = await PatchJsonAsync(
                $"/api/appointments/{appointment.Id}/complete",
                new CompleteAppointmentDto { Notes = "Checkup done" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var completed = await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
            Assert.NotNull(completed);
            Assert.True(completed.IsCompleted);
            Assert.Equal("Checkup done", completed.Notes);
        }

        [Fact]
        public async Task Complete_NotesLongerThanLimit_Returns400()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            var appointment = await CreateAppointmentAsync(doctor.Id, patient.Id);

            var response = await PatchJsonAsync(
                $"/api/appointments/{appointment.Id}/complete",
                new CompleteAppointmentDto { Notes = new string('a', 1001) });

            var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
            AssertValidationErrorFor(problem, "Notes");
        }

        [Fact]
        public async Task Delete_ExistingAppointment_Returns204AndAppointmentIsGone()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            var appointment = await CreateAppointmentAsync(doctor.Id, patient.Id);

            var response = await Client.DeleteAsync($"/api/appointments/{appointment.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var afterDelete = await Client.GetAsync($"/api/appointments/{appointment.Id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        }

        [Fact]
        public async Task Delete_NonExistingAppointment_Returns404()
        {
            var response = await Client.DeleteAsync("/api/appointments/999");

            await AssertProblemAsync(response, HttpStatusCode.NotFound);
        }
    }
}