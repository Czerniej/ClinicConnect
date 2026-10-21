using System.Net;
using System.Net.Http.Json;

using ClinicConnectApi.Dtos;
using ClinicConnectApi.Tests.Infrastructure;

namespace ClinicConnectApi.Tests.Api
{
    public class PatientsApiTests : ApiTestBase
    {
        private static CreatePatientDto Dto(string firstName = "Brandon", string pesel = ValidPesel) => new()
        {
            FirstName = firstName,
            LastName = "Stark",
            Pesel = pesel
        };

        [Fact]
        public async Task Create_ValidPatient_Returns201WithWorkingLocation()
        {
            var response = await Client.PostAsJsonAsync("/api/patients", Dto());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<PatientResponseDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            Assert.EndsWith($"/api/Patients/{created.Id}",
                response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);

            var fetched = await Client.GetAsync(response.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        }

        [Fact]
        public async Task Create_InvalidPesel_Returns400WithPeselError()
        {
            var response = await Client.PostAsJsonAsync("/api/patients", Dto(pesel: "12345678901"));

            var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
            AssertValidationErrorFor(problem, "Pesel");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_BlankFirstName_Returns400(string firstName)
        {
            var response = await Client.PostAsJsonAsync("/api/patients", Dto(firstName: firstName));

            var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
            AssertValidationErrorFor(problem, "FirstName");
        }

        [Fact]
        public async Task Create_DuplicatePesel_Returns409ProblemDetails()
        {
            await Client.PostAsJsonAsync("/api/patients", Dto());

            var response = await Client.PostAsJsonAsync("/api/patients", Dto());

            await AssertProblemAsync(response, HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetById_NonExistingId_Returns404ProblemDetails()
        {
            var response = await Client.GetAsync("/api/patients/999");

            await AssertProblemAsync(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetById_NonNumericId_Returns404()
        {
            var response = await Client.GetAsync("/api/patients/abc");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ExistingPatient_Returns204AndPatientIsGone()
        {
            var patient = await CreatePatientAsync();

            var response = await Client.DeleteAsync($"/api/patients/{patient.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var afterDelete = await Client.GetAsync($"/api/patients/{patient.Id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        }

        [Fact]
        public async Task Delete_PatientWithAppointment_Returns409AndPatientRemains()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            await CreateAppointmentAsync(doctor.Id, patient.Id);

            var response = await Client.DeleteAsync($"/api/patients/{patient.Id}");

            await AssertProblemAsync(response, HttpStatusCode.Conflict);
            var stillThere = await Client.GetAsync($"/api/patients/{patient.Id}");
            Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
        }
    }
}