using TinyEndpointsWeb.Services.Models;

namespace TinyEndpointsWeb.Services;

public interface IProductService
{
    IReadOnlyList<Product> GetAll();
    Product GetById(string id);
}
