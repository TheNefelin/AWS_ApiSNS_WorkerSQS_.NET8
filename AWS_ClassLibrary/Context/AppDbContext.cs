using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWS_ClassLibrary.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> companies { get; set; }
    public DbSet<Product> products { get; set; }

    public async Task<IEnumerable<CompanyDTO>> GetCompanies()
    {
        try
        {
            var entities = await companies.ToListAsync();

            return entities.Select(e => new CompanyDTO
            {
                Company_id = e.Company_id,
                Name = e.Name,
                Email = e.Email,
                Img = e.Img
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<CompanyDTO?> GetCompanyRandom()
    {
        try
        {
            var entity = await companies.OrderBy(c => Guid.NewGuid()).FirstOrDefaultAsync();

            if (entity != null)
            {
                return new CompanyDTO
                {
                    Company_id = entity.Company_id,
                    Name = entity.Name,
                    Email = entity.Email,
                    Img = entity.Img
                };
            }

            return null;
        }
        catch
        {
            throw;
        }
    }

    public async Task<IEnumerable<ProductDTO>> GetProducts()
    {
        try
        {
            var entities = await products.ToListAsync();

            return entities.Select(e => new ProductDTO
            {
                Product_id = e.Product_id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<IEnumerable<ProductDTO>> GetProdctsByAmount(int amount)
    {
        try
        {
            var entities = await products.OrderBy(p => Guid.NewGuid()).Take(amount).ToListAsync();

            return entities.Select(e => new ProductDTO
            {
                Product_id = e.Product_id,
                Name = e.Name,
                Description = e.Description,
                Price = e.Price
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}