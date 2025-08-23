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
        var entity = await companies.ToListAsync();

        return entity.Select(e => new CompanyDTO
        {
            Company_id = e.Company_id,
            Name = e.Name,
            Email = e.Email,
            Img = e.Img
        });
    }

    public async Task<IEnumerable<ProductDTO>> GetProducts()
    {
        var entity = await products.ToListAsync();

        return entity.Select(e => new ProductDTO
        {
            Product_id = e.Product_id,
            Name = e.Name,
            Description = e.Description,
            Price = e.Price
        });
    }
}