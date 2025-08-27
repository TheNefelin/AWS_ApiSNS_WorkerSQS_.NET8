using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ILogger<ProductRepository> _logger;
    private readonly AppDbContext _context;

    public ProductRepository(ILogger<ProductRepository> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
    {
        try
        {
            var entities = await _context.products.ToListAsync();

            var products = entities.Select(e => new ProductDTO
            {
                Product_id = e.Product_id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price
            });

            _logger.LogDebug("Obtenidos {Count} productos de la base de datos", products.Count());
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todos los productos");
            throw;
        }
    }

    public async Task<IEnumerable<ProductDTO>> GetRandomProductsAsync(int amount)
    {
        try
        {
            if (amount <= 0)
            {
                _logger.LogWarning("Cantidad solicitada inválida: {Amount}", amount);
                return Enumerable.Empty<ProductDTO>();
            }

            var entities = await _context.products
                .OrderBy(p => Guid.NewGuid())
                .Take(amount)
                .ToListAsync();

            var products = entities.Select(e => new ProductDTO
            {
                Product_id = e.Product_id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price
            });

            _logger.LogDebug("Obtenidos {Count} productos aleatorios (solicitados: {RequestedAmount})",
                products.Count(), amount);

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo productos aleatorios (cantidad: {Amount})", amount);
            throw;
        }
    }

    public async Task<ProductDTO?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var entity = await _context.products
                .FirstOrDefaultAsync(p => p.Product_id == productId);

            if (entity == null)
            {
                _logger.LogDebug("Producto no encontrado con ID: {ProductId}", productId);
                return null;
            }

            var product = new ProductDTO
            {
                Product_id = entity.Product_id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price
            };

            _logger.LogDebug("Producto encontrado: {ProductName}", product.Name);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo producto por ID: {ProductId}", productId);
            throw;
        }
    }

    public async Task<IEnumerable<ProductDTO>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        try
        {
            if (!productIds.Any())
            {
                _logger.LogDebug("Lista de IDs de productos vacía");
                return Enumerable.Empty<ProductDTO>();
            }

            var entities = await _context.products
                .Where(p => productIds.Contains(p.Product_id))
                .ToListAsync();

            var products = entities.Select(e => new ProductDTO
            {
                Product_id = e.Product_id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price
            });

            _logger.LogDebug("Obtenidos {Count} productos por IDs (solicitados: {RequestedCount})",
                products.Count(), productIds.Count());

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo productos por IDs");
            throw;
        }
    }
}