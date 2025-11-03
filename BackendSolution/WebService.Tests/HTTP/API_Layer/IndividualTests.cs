/* This file documents a representative integration test for the endpoints of the Individual entity.
   The purpose is to ensure that the API behaves as expected when interacting with Individual resources. */

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace WebService.Tests.HTTP.API_Layer;

public class IndividualApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IndividualApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    // Here, we test if the HTTP responses from the Individual API endpoints are successful
    // and if they return the correct content type (application/json).
    [Theory]
    [InlineData("/api/individual/nm0000001")] 
    [InlineData("/api/individual/search?name=Tom")]
    public async Task Get_IndividualEndpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.True(response.IsSuccessStatusCode); // 200-299 status code response
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType); // json response
    }

    // Here, we test the API's behavior for error handling when requesting a non-existent individual.
    [Fact] 
    public async Task Get_NonExistentIndividual_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/individual/nm9999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); // 404 Not Found response
    }
}