using System.Net;
using System.Net.Http.Json;

using ClinicConnectApi.Dtos;
using ClinicConnectApi.Tests.Infrastructure;

namespace ClinicConnectApi.Tests.Api
{
    public class DoctorsApiTests : ApiTestBase
    {
        private static CreateDoctorDto Dto(decimal fee = 200m) => new()
        {
            FirstName = "Lisa",
            LastName = "Cuddy",
            Specialization = "Endocrinologist",
            ConsultationFee = fee
        };

        [Fact]
        public async Task Create_ValidDoctor_Returns201WithWorkingLocation()
        {
            var response = await Client.PostAsJsonAsync("/api/doctors", Dto());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<DoctorResponseDto>();
            Assert.NotNull(created);
            Assert.True(created.Id > 0);
            Assert.EndsWith($"/api/Doctors/{created.Id}",
                response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);

            var fetched = await Client.GetAsync(response.Headers.Location);
            Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task Create_NonPositiveFee_Returns400(int fee)
        {
            var response = await Client.PostAsJsonAsync("/api/doctors", Dto(fee));

            var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
            AssertValidationErrorFor(problem, "ConsultationFee");
        }

        [Fact]
        public async Task GetById_NonExistingId_Returns404ProblemDetails()
        {
            var response = await Client.GetAsync("/api/doctors/999");

            await AssertProblemAsync(response, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_ExistingDoctor_Returns204AndDoctorIsGone()
        {
            var doctor = await CreateDoctorAsync();

            var response = await Client.DeleteAsync($"/api/doctors/{doctor.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var afterDelete = await Client.GetAsync($"/api/doctors/{doctor.Id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        }

        [Fact]
        public async Task Delete_DoctorWithAppointment_Returns409AndDoctorRemains()
        {
            var doctor = await CreateDoctorAsync();
            var patient = await CreatePatientAsync();
            await CreateAppointmentAsync(doctor.Id, patient.Id);

            var response = await Client.DeleteAsync($"/api/doctors/{doctor.Id}");

            await AssertProblemAsync(response, HttpStatusCode.Conflict);
            var stillThere = await Client.GetAsync($"/api/doctors/{doctor.Id}");
            Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
        }
    }
}