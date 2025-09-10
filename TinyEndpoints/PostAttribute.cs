namespace TinyEndpoints;

public class PostAttribute : EndpointAttribute
{
    public PostAttribute(string route) : base(route) { }
}

public class PostAttribute<TConfigurator> : EndpointAttribute<TConfigurator> where TConfigurator : IEndpointConfigurator
{
    public PostAttribute(string route) : base(route) { }
}