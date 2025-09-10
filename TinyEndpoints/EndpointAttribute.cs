namespace TinyEndpoints;

public abstract class EndpointAttribute : Attribute
{
    public EndpointAttribute(string route)
    {
        Route = route;
    }

    public string Route { get; }

    public Type? ConfiguratorType { get; set; }

    public string? Name { get; set; }
}

public abstract class EndpointAttribute<TConfigurator> : EndpointAttribute where TConfigurator : IEndpointConfigurator
{
    public EndpointAttribute(string route) : base(route)
    {
        ConfiguratorType = typeof(TConfigurator);
    }
}
