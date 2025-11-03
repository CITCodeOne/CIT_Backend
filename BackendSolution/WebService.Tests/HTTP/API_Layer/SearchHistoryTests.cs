/* This file documents a representative integration test for the endpoints of the SearchHistory entity.
   The purpose is to ensure that the API behaves as expected when interacting with SearchHistory resources. */

using Microsoft.AspNetCore.Mvc.Testing;

namespace WebService.Tests.HTTP.API_Layer;

public class SearchHistoryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SearchHistoryApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Here, we test if the HTTP responses from the SearchHistory API endpoints are successful
    // and if they return the correct content type (application/json).
    [Theory]
    [InlineData("/api/user/1/searchhistory")]
    public async Task Get_SearchHistoryEndpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.True(response.IsSuccessStatusCode); // Status Code 200-299
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    // Here, we test if the API correctly retrieves an individual's search history entry by its ID.
    [Fact]
    public async Task Get_Individual_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/api/individual/nm0000001");

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}