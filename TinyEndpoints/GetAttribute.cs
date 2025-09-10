namespace TinyEndpoints;

public class GetAttribute : EndpointAttribute
{
    public GetAttribute(string route) : base(route)
    {
    }
}
public class GetAttribute<TConfigurator> : EndpointAttribute<TConfigurator> where TConfigurator : IEndpointConfigurator
{
    public GetAttribute(string route) : base(route) { }
}