using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TinyEndpoints.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Here you can replace services with fakes/mocks if needed.
            // Example:
            // var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IProductService));
            // if (descriptor != null) services.Remove(descriptor);
            // services.AddSingleton<IProductService, FakeProductService>();
        });
    }
}
