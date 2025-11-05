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

    // Titles Tests

    // Get Titles Test (just checking for OK response)
    [Fact]
    public async Task Get_Titles_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/titles");
        // Act
        var response = await _client.SendAsync(request);
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Get Title by ID Test (checks that a specific title can be retrieved and has the correct name)
    [Fact]
    public async Task Get_TitleById_ReturnsCorrectTitle()
    {
        // Arrange
        var titleId = "tt0063929 "; // title ID for "Monty Python's Flying Circus"
        var expectedTitleName = "Monty Python's Flying Circus";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/titles/{titleId}");
        // Act
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        // Assert
        Assert.Contains(expectedTitleName, content); // Check that the title name is in the response
    }
}
