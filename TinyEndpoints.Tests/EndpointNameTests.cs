using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace TinyEndpoints.Tests;

public class EndpointNameTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EndpointNameTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task NamedEndpoint_ShouldHaveNameMetadata()
    {
        var response = await _client.GetAsync("/products/named");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Inspect the server's endpoint data source to verify WithName applied
        var dataSources = _factory.Services.GetServices(typeof(EndpointDataSource)).Cast<EndpointDataSource>();
        var found = dataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .Any(e => string.Equals(e.DisplayName, "NamedProducts", StringComparison.Ordinal) ||
                      e.Metadata.OfType<Microsoft.AspNetCore.Routing.RouteNameMetadata>().Any(m => m.RouteName == "NamedProducts"));
        Assert.True(found, "Expected to find endpoint with name 'NamedProducts'.");
    }
}
