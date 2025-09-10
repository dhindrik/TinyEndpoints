using TinyEndpointsWeb.Services.Models;

namespace TinyEndpointsWeb.Services;

public class ProductService : IProductService
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = "1", Title = "Spinning Rod", Description = "Lightweight spinning rod for freshwater sportfishing." },
        new Product { Id = "2", Title = "Baitcasting Reel", Description = "High-performance baitcasting reel for precision casting." },
        new Product { Id = "3", Title = "Braided Fishing Line", Description = "Strong braided line, 300m spool, 20lb test." },
        new Product { Id = "4", Title = "Topwater Lure", Description = "Realistic topwater lure for bass and pike." },
        new Product { Id = "5", Title = "Tackle Box", Description = "Waterproof tackle box with adjustable compartments." }
    };

    public IReadOnlyList<Product> GetAll()
    {
        return _products;
    }

    public Product GetById(string id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id '{id}' not found.");
        }

        return product;
    }
}
