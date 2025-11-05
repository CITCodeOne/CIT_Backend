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
