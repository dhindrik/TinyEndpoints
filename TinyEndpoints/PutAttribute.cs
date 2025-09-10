namespace TinyEndpoints;

public class PutAttribute : EndpointAttribute
{
    public PutAttribute(string route) : base(route) { }
}

public class PutAttribute<TConfigurator> : EndpointAttribute<TConfigurator> where TConfigurator : IEndpointConfigurator
{
    public PutAttribute(string route) : base(route) { }
}

