using System.Net;

namespace WebServiceLayerTests;

public class WSLTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public WSLTests(WebApplicationFactoryFixture fixture)
    {
        _client = fixture.Client;
    }

    // Health Check Test
    [Fact]
    public async Task Get_HealthCheck_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/health");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
