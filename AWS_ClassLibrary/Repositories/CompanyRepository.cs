using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ILogger<CompanyRepository> _logger;
    private readonly AppDbContext _context;

    public CompanyRepository(ILogger<CompanyRepository> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IEnumerable<CompanyDTO>> GetAllCompaniesAsync()
    {
        try
        {
            var entities = await _context.companies.ToListAsync();

            var companies = entities.Select(e => new CompanyDTO
            {
                Company_id = e.Company_id,
                Name = e.Name,
                Email = e.Email,
                Img = e.Img
            });

            _logger.LogDebug("Obtenidas {Count} empresas de la base de datos", companies.Count());
            return companies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todas las empresas");
            throw;
        }
    }

    public async Task<CompanyDTO?> GetRandomCompanyAsync()
    {
        try
        {
            var entity = await _context.companies
                .OrderBy(c => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                _logger.LogWarning("No se encontraron empresas en la base de datos");
                return null;
            }

            var company = new CompanyDTO
            {
                Company_id = entity.Company_id,
                Name = entity.Name,
                Email = entity.Email,
                Img = entity.Img
            };

            _logger.LogDebug("Empresa aleatoria obtenida: {CompanyName}", company.Name);
            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo empresa aleatoria");
            throw;
        }
    }

    public async Task<CompanyDTO?> GetCompanyByIdAsync(Guid companyId)
    {
        try
        {
            var entity = await _context.companies
                .FirstOrDefaultAsync(c => c.Company_id == companyId);

            if (entity == null)
            {
                _logger.LogDebug("Empresa no encontrada con ID: {CompanyId}", companyId);
                return null;
            }

            var company = new CompanyDTO
            {
                Company_id = entity.Company_id,
                Name = entity.Name,
                Email = entity.Email,
                Img = entity.Img
            };

            _logger.LogDebug("Empresa encontrada: {CompanyName}", company.Name);
            return company;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo empresa por ID: {CompanyId}", companyId);
            throw;
        }
    }
}