namespace TinyEndpoints;

using Microsoft.AspNetCore.Builder;

public interface IEndpointConfigurator
{
    void Configure(RouteHandlerBuilder builder);
}
