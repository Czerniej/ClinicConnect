using System.Net;

using ClinicConnectApi.Tests.Infrastructure;

namespace ClinicConnectApi.Tests.Api
{
    public class OpenApiTests : ApiTestBase
    {
        [Fact]
        public async Task OpenApiDocument_IsAvailableAndDescribesAppointments()
        {
            var response = await Client.GetAsync("/openapi/v1.json");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var document = await ReadJsonAsync(response);
            Assert.True(document.GetProperty("paths").TryGetProperty("/api/Appointments", out _));
        }
    }
}