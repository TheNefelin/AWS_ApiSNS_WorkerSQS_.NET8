using AWS_ClassLibrary.DTOs;

namespace AWS_ClassLibrary.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
    Task<IEnumerable<ProductDTO>> GetRandomProductsAsync(int amount);
    Task<ProductDTO?> GetProductByIdAsync(Guid productId);
    Task<IEnumerable<ProductDTO>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
}