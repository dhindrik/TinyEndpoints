# TinyEndpoints

Lightweight, attribute-driven endpoint registration for ASP.NET Core Minimal APIs powered by a Roslyn source generator.

You decorate methods with HTTP verb attributes (`Get`, `Post`, `Put`, `Delete`). A source generator discovers them at build time and emits registration code you invoke at startup.


## Quick Start
1. Add a partial `Program` annotated with `[GenerateEndpoints]`:
```csharp
using TinyEndpoints;

[GenerateEndpoints]
public partial class Program { }
```
2. In your main `Program.cs` after building middleware / before `app.Run()` call:
```csharp
RegisterGeneratedEndpoints(app);
```
3. Create an endpoints class with methods (static or instance) decorated with verb attributes:
```csharp
using TinyEndpoints;

public class UserEndpoints
{
    [Get("users/{id}")]
    public IResult GetUser(IUserService svc, string id) =>
        svc.TryGet(id, out var u) ? Results.Ok(u) : Results.NotFound();

    [Post<AuthConfigurator>("member/users")]
    public IResult CreateUser(IUserService svc, CreateUser dto) =>
        Results.Created($"/users/{dto.Id}", dto);
}
```
4. Build & run. The generator emits `RegisterGeneratedEndpoints` which wires up `MapGet`, `MapPost`, etc.

## Supported Attributes
```csharp
GetAttribute
GetAttribute<TConfigurator>
PostAttribute
PostAttribute<TConfigurator>
PutAttribute
PutAttribute<TConfigurator>
DeleteAttribute
DeleteAttribute<TConfigurator>
```
Each constructor takes a single route string. Generic form auto-assigns the configurator type; non-generic form can set `ConfiguratorType = typeof(MyConfigurator)` optionally.

## Configurators (Per-endpoint customization)
Implement `IEndpointConfigurator`:
```csharp
public class AuthConfigurator : IEndpointConfigurator
{
    public void Configure(RouteHandlerBuilder builder)
        => builder.RequireAuthorization();
}
```
Use either style:
```csharp
[Get<AuthConfigurator>("member/profile")]
// or
[Get("member/profile", ConfiguratorType = typeof(AuthConfigurator))]
```
Inside `Configure` you can call any `RouteHandlerBuilder` methods: `RequireAuthorization`, `AddEndpointFilter`, `WithName`, `WithTags`, `Produces`, etc.


## Parameter Binding & DI
Matches standard Minimal APIs:
- Constructor parameters of handlers are resolved from DI by type.
- Route parameters bound by name.
- Complex body parameters for POST/PUT.
- Source-specific attributes like `[FromHeader]`, `[FromQuery]`, etc. honored.
- Return types: `IResult`, POCO/value, `Task` / `ValueTask` variants.

## Example
```csharp
public class ProductEndpoints
{
    [Get("products/{id}")]
    public static IResult GetById(IProductService products, string id)
        => products.GetByIdOrNull(id) is { } p ? Results.Ok(p) : Results.NotFound();

    [Post<AuthConfigurator>("member/products")]
    public IResult Create(IProductService products, CreateProduct dto)
        => Results.Created($"/products/{dto.Id}", products.Add(dto));
}
```