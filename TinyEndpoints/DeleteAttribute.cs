namespace TinyEndpoints;

public class DeleteAttribute : EndpointAttribute
{
    public DeleteAttribute(string route) : base(route) { }
}

public class DeleteAttribute<TConfigurator> : EndpointAttribute<TConfigurator> where TConfigurator : IEndpointConfigurator
{
    public DeleteAttribute(string route) : base(route) { }
}

