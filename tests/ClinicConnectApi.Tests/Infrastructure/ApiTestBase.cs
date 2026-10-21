using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using ClinicConnectApi.Dtos;

namespace ClinicConnectApi.Tests.Infrastructure
{
    public abstract class ApiTestBase : IDisposable
    {
        protected const string ValidPesel = "01320215673";
        protected static readonly DateTime FutureSlot = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);

        protected ApiFactory Factory { get; } = new();
        protected HttpClient Client { get; }

        protected ApiTestBase()
        {
            Client = Factory.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            Factory.Dispose();
        }

        protected async Task<DoctorResponseDto> CreateDoctorAsync(string lastName = "House")
        {
            var response = await Client.PostAsJsonAsync("/api/doctors", new CreateDoctorDto
            {
                FirstName = "Gregory",
                LastName = lastName,
                Specialization = "Pathologist",
                ConsultationFee = 200m
            });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<DoctorResponseDto>())!;
        }

        protected async Task<PatientResponseDto> CreatePatientAsync(string pesel = ValidPesel)
        {
            var response = await Client.PostAsJsonAsync("/api/patients", new CreatePatientDto
            {
                FirstName = "Brandon",
                LastName = "Stark",
                Pesel = pesel
            });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<PatientResponseDto>())!;
        }

        protected async Task<AppointmentResponseDto> CreateAppointmentAsync(
            int doctorId, int patientId, DateTime? date = null)
        {
            var response = await Client.PostAsJsonAsync("/api/appointments", new CreateAppointmentDto
            {
                DoctorId = doctorId,
                PatientId = patientId,
                AppointmentDate = date ?? FutureSlot
            });
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<AppointmentResponseDto>())!;
        }

        protected Task<HttpResponseMessage> PatchJsonAsync(string url, object body)
            => Client.PatchAsync(url, JsonContent.Create(body));

        protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(body);
            return document.RootElement.Clone();
        }

        protected static async Task<JsonElement> AssertProblemAsync(
            HttpResponseMessage response, HttpStatusCode expectedStatus)
        {
            Assert.Equal(expectedStatus, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await ReadJsonAsync(response);
            Assert.Equal((int)expectedStatus, problem.GetProperty("status").GetInt32());
            return problem;
        }

        protected static void AssertValidationErrorFor(JsonElement problem, string field)
        {
            var errorKeys = problem.GetProperty("errors").EnumerateObject().Select(p => p.Name);
            Assert.Contains(errorKeys, key => key.Equals(field, StringComparison.OrdinalIgnoreCase));
        }
    }
}