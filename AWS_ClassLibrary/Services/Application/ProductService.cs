using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Repositories;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services.Application;

public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    private readonly IProductRepository _productRepository;

    public ProductService(ILogger<ProductService> logger, IProductRepository productRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<IEnumerable<ProductDTO>>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productRepository.GetAllProductsAsync();

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Productos obtenidos exitosamente.",
                Data = products
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener productos");

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener los productos.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDTO>>> GetRandomProductsAsync(int amount)
    {
        try
        {
            if (amount < 1 || amount > 10)
            {
                return new ApiResponse<IEnumerable<ProductDTO>>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "La cantidad debe estar entre 1 y 10.",
                    Data = null
                };
            }

            var products = await _productRepository.GetRandomProductsAsync(amount);

            if (!products.Any())
            {
                return new ApiResponse<IEnumerable<ProductDTO>>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "No se encontraron productos disponibles.",
                    Data = null
                };
            }

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = $"{products.Count()} productos obtenidos exitosamente.",
                Data = products
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener productos aleatorios (cantidad: {Amount})", amount);

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener productos aleatorios.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<ProductDTO>> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(productId);

            if (product == null)
            {
                return new ApiResponse<ProductDTO>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = $"Producto no encontrado con ID: {productId}",
                    Data = null
                };
            }

            return new ApiResponse<ProductDTO>
            {
                Success = true,
                StatusCode = 200,
                Message = "Producto obtenido exitosamente.",
                Data = product
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener producto por ID: {ProductId}", productId);

            return new ApiResponse<ProductDTO>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener el producto.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDTO>>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        try
        {
            if (!productIds.Any())
            {
                return new ApiResponse<IEnumerable<ProductDTO>>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Lista de IDs de productos no puede estar vacía.",
                    Data = null
                };
            }

            var products = await _productRepository.GetProductsByIdsAsync(productIds);

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = $"{products.Count()} productos obtenidos exitosamente.",
                Data = products
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener productos por IDs");

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener productos por IDs.",
                Data = null
            };
        }
    }
}