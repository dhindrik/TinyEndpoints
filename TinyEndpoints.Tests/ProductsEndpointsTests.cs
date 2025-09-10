using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using System.Text.Json;
using TinyEndpointsWeb.Services.Models;
using TinyEndpointsWeb.Endpoints;


namespace TinyEndpoints.Tests;

public class ProductsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetAll_ShouldReturnProducts()
    {
        var products = await _client.GetFromJsonAsync<List<Product>>("/products/all");
        Assert.NotNull(products);
        Assert.NotEmpty(products!);
    }

    [Fact]
    public async Task GetById_ShouldReturnSingleProduct()
    {
        var response = await _client.GetAsync("/products/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(product);
        Assert.Equal("1", product!.Id);
    }

    [Fact]
    public async Task GetById_NotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync("/products/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Buy_ShouldBindHeaderAndBody()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/products/buy");
        request.Headers.Add("UserId", "123");
        request.Content = JsonContent.Create(new BuyRequest(7, 2));

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<BuyResponse>();
        Assert.NotNull(data);
        Assert.Equal(123, data!.UserId);
        Assert.Equal(7, data.ProductId);
        Assert.Equal(2, data.Quantity);
    }

    [Fact]
    public async Task MemberProducts_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.GetAsync("/member/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MemberProducts_WithAuth_ShouldReturnProducts()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/member/products");
        request.Headers.Add("X-Test-User", "alice");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        Assert.NotNull(products);
        Assert.NotEmpty(products!);
    }

    // --- Additional endpoint coverage ---

    [Fact]
    public async Task MemberBuy_WithAuth_ShouldReturnBuyResponse()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/member/products/buy");
        request.Headers.Add("X-Test-User", "alice");
        request.Headers.Add("UserId", "77");
        request.Content = JsonContent.Create(new BuyRequest(3, 5));
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<BuyResponse>();
        Assert.NotNull(data);
        Assert.Equal(77, data!.UserId);
        Assert.Equal(3, data.ProductId);
        Assert.Equal(5, data.Quantity);
    }

    [Fact]
    public async Task MemberBuy_WithoutAuth_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/member/products/buy");
        request.Headers.Add("UserId", "11");
        request.Content = JsonContent.Create(new BuyRequest(2, 1));
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTitle_ShouldReturnUpdatedProduct()
    {
        var payload = new { Title = "Updated Title" };
        var response = await _client.PutAsJsonAsync("/products/1/title", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task UpdateTitle_NotFound_ShouldReturn404()
    {
        var payload = new { Title = "Does Not Matter" };
        var response = await _client.PutAsJsonAsync("/products/999/title", payload);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMemberNote_WithAuth_ShouldReturnNote()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/member/products/1/note");
        request.Headers.Add("X-Test-User", "alice");
        request.Content = JsonContent.Create(new { Note = "Great product" });
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        string? noteValue = null;
        if (json.TryGetProperty("Note", out var notePropUpper)) noteValue = notePropUpper.GetString();
        else if (json.TryGetProperty("note", out var notePropLower)) noteValue = notePropLower.GetString();
        Assert.Equal("Great product", noteValue);
    }

    [Fact]
    public async Task UpdateMemberNote_WithoutAuth_ShouldReturn401()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/member/products/1/note");
        request.Content = JsonContent.Create(new { Note = "No auth" });
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturn204()
    {
        var response = await _client.DeleteAsync("/products/1");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_NotFound_ShouldReturn404()
    {
        var response = await _client.DeleteAsync("/products/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMemberProduct_WithAuth_ShouldReturn204()
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "/member/products/1");
        request.Headers.Add("X-Test-User", "alice");
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMemberProduct_WithoutAuth_ShouldReturn401()
    {
        var response = await _client.DeleteAsync("/member/products/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public record BuyResponse(int UserId, int ProductId, int Quantity);
