using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebServiceLayerTests;

public class WSLTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    // Constructor to initialize HttpClient from the fixture
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

    [Fact]
    public async Task Post_Ratings_WithValidToken_CreatesRating()
    {
        var authContext = await RegisterAndAuthenticateAsync();

        var ratingPayload = new
        {
            TitleId = "tt0063929",
            Rating = 8
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/ratings")
        {
            Content = CreateJsonContent(ratingPayload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authContext.Token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Ratings_WithValidToken_RemovesRating()
    {
        var authContext = await RegisterAndAuthenticateAsync();

        await CreateRatingAsync(authContext.Token, "tt0063929", 7);

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/ratings/tt0063929");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authContext.Token);

        var deleteResponse = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    private static StringContent CreateJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<(string Username, string Password, string Token)> RegisterAndAuthenticateAsync()
    {
        var unique = Guid.NewGuid().ToString("N");
        var username = $"testuser_{unique}";
        var password = "Sup3rSecret!";
        var registrationPayload = new
        {
            Name = $"Test User {unique}",
            Username = username,
            Email = $"test_{unique}@example.com",
            Password = password
        };

        var registerResponse = await _client.PostAsync("/api/user", CreateJsonContent(registrationPayload));
        registerResponse.EnsureSuccessStatusCode();

        var loginPayload = new { Username = username, Password = password };
        var loginResponse = await _client.PostAsync("/api/user/login", CreateJsonContent(loginPayload));
        loginResponse.EnsureSuccessStatusCode();

        var loginJson = await loginResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(loginJson);
        var token = document.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Token missing from login response");

        return (username, password, token);
    }

    private async Task CreateRatingAsync(string token, string titleId, int rating)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/ratings")
        {
            Content = CreateJsonContent(new { TitleId = titleId, Rating = rating })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
