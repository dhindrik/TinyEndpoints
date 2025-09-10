using System;

namespace TinyEndpointsWeb.Services.Models;

public class Product
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
}
