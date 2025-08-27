using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWS_ClassLibrary.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> companies { get; set; }
    public DbSet<Product> products { get; set; }
}