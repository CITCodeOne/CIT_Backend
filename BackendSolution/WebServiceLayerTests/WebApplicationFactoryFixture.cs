using Microsoft.AspNetCore.Mvc.Testing;

namespace WebServiceLayerTests;

public class WebApplicationFactoryFixture : IDisposable
{
    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }

    public WebApplicationFactoryFixture()
    {
        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}

// INFO:
// This fixture sets up a WebApplicationFactory for the ASP.NET Core application defined in the Program class.
// It creates an HttpClient that can be used to send requests to the in-memory test server.
// The Dispose method ensures that resources are cleaned up after tests are completed.
// This fixture can be used in test classes by implementing IClassFixture<WebApplicationFactoryFixture>,
// allowing tests to share the same setup and teardown logic.
// As far as i understand, the Dispose method is called automatically by the test framework when the tests are done using the fixture.
// This ensures that the HttpClient and WebApplicationFactory are properly disposed of, preventing "resource leaks".
