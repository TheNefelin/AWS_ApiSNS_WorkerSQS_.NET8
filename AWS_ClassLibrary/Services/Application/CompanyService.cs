using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Repositories;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services.Application;

public class CompanyService : ICompanyService
{
    private readonly ILogger<CompanyService> _logger;
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ILogger<CompanyService> logger, ICompanyRepository companyRepository)
    {
        _logger = logger;
        _companyRepository = companyRepository;
    }

    public async Task<ApiResponse<IEnumerable<CompanyDTO>>> GetAllCompaniesAsync()
    {
        try
        {
            var companies = await _companyRepository.GetAllCompaniesAsync();

            return new ApiResponse<IEnumerable<CompanyDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Empresas obtenidas exitosamente.",
                Data = companies
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener empresas");

            return new ApiResponse<IEnumerable<CompanyDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener las empresas.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<CompanyDTO>> GetRandomCompanyAsync()
    {
        try
        {
            var company = await _companyRepository.GetRandomCompanyAsync();

            if (company == null)
            {
                return new ApiResponse<CompanyDTO>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "No se encontraron empresas disponibles.",
                    Data = null
                };
            }

            return new ApiResponse<CompanyDTO>
            {
                Success = true,
                StatusCode = 200,
                Message = "Empresa aleatoria obtenida exitosamente.",
                Data = company
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener empresa aleatoria");

            return new ApiResponse<CompanyDTO>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener empresa aleatoria.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<CompanyDTO>> GetCompanyByIdAsync(Guid companyId)
    {
        try
        {
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            if (company == null)
            {
                return new ApiResponse<CompanyDTO>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = $"Empresa no encontrada con ID: {companyId}",
                    Data = null
                };
            }

            return new ApiResponse<CompanyDTO>
            {
                Success = true,
                StatusCode = 200,
                Message = "Empresa obtenida exitosamente.",
                Data = company
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio al obtener empresa por ID: {CompanyId}", companyId);

            return new ApiResponse<CompanyDTO>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno al obtener la empresa.",
                Data = null
            };
        }
    }
}
