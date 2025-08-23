using AWS_ClassLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWS_ClassLibrary.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> companies { get; set; }
    public DbSet<Product> products { get; set; }

    public async Task<IEnumerable<Company>> GetCompanies()
    {
        return await this.companies.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProducts()
    {
        return await this.products.ToListAsync();
    }
}