using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;

namespace AWS_ClassLibrary.Services.Application;

public interface IProductService
{
    Task<ApiResponse<IEnumerable<ProductDTO>>> GetAllProductsAsync();
    Task<ApiResponse<IEnumerable<ProductDTO>>> GetRandomProductsAsync(int amount);
    Task<ApiResponse<ProductDTO>> GetProductByIdAsync(Guid productId);
    Task<ApiResponse<IEnumerable<ProductDTO>>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
}