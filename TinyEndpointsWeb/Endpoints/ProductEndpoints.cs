using Microsoft.AspNetCore.Mvc;
using TinyEndpoints; // Added for attribute & configurator types
using TinyEndpointsWeb.Services; // corrected
using TinyEndpointsWeb.Services.Models; // corrected

namespace TinyEndpointsWeb.Endpoints; // align with tests

public class ProductEndpoints
{
    [Get("products/all")]
    public IReadOnlyList<Product> GetByAll(IProductService productService)
    {
        var products = productService.GetAll();
        return products;
    }

    [Get("products/{id}")]
    public static IResult GetById(IProductService productService, string id)
    {
        try
        {
            var product = productService.GetById(id);
            return Results.Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }

    [Post("products/buy")]
    public IResult Buy(IProductService productService, [FromHeader] int UserId, BuyRequest buyRequest)
    {
        var uid = UserId; // header binding
        var response = new BuyResponse(uid, buyRequest.Id, buyRequest.Quantity);
        return Results.Ok(response);
    }

    [Get<AuthicateConfigurator>("member/products")]
    public IReadOnlyList<Product> GetMemberProducts(IProductService productService)
    {
        var products = productService.GetAll();
        return products;
    }

    // --- Additional endpoints to cover remaining attribute types ---

    // PostAttribute<TConfigurator>
    [Post<AuthicateConfigurator>("member/products/buy")]
    public IResult BuyMember(IProductService productService, [FromHeader] int UserId, BuyRequest buyRequest)
    {
        var response = new BuyResponse(UserId, buyRequest.Id, buyRequest.Quantity);
        return Results.Ok(response);
    }

    // PutAttribute (non-generic)
    [Put("products/{id}/title")]
    public IResult UpdateTitle(IProductService productService, string id, UpdateTitleRequest request)
    {
        try
        {
            var product = productService.GetById(id);
            // Simulate update (no persistence in demo)
            var updated = new Product { Id = product.Id, Title = request.Title, Description = product.Description };
            return Results.Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }

    // PutAttribute<TConfigurator>
    [Put<AuthicateConfigurator>("member/products/{id}/note")]
    public IResult UpdateMemberNote(IProductService productService, string id, UpdateNoteRequest request)
    {
        try
        {
            // Just validate product exists
            _ = productService.GetById(id);
            return Results.Ok(new { Id = id, Note = request.Note });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }

    // DeleteAttribute (non-generic)
    [Delete("products/{id}")]
    public IResult DeleteProduct(IProductService productService, string id)
    {
        try
        {
            _ = productService.GetById(id); // existence check
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }

    // DeleteAttribute<TConfigurator>
    [Delete<AuthicateConfigurator>("member/products/{id}")]
    public IResult DeleteMemberProduct(IProductService productService, string id)
    {
        try
        {
            _ = productService.GetById(id); // existence check
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
    }

    // New endpoint with Name property to verify generator adds WithName
    [Get("products/named", Name = "NamedProducts")]
    public static IResult GetNamedEndpoint()
    {
        return Results.Ok(new { Status = "ok" });
    }
}

public record BuyRequest(int Id, int Quantity);
public record BuyResponse(int UserId, int ProductId, int Quantity);
public record UpdateTitleRequest(string Title);
public record UpdateNoteRequest(string Note);

public class AuthicateConfigurator : IEndpointConfigurator
{
    public void Configure(RouteHandlerBuilder builder)
    {
        builder.RequireAuthorization();
    }
}

