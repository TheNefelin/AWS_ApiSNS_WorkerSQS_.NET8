using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;

namespace AWS_ClassLibrary.Services.Application;

public interface ICompanyService
{
    Task<ApiResponse<IEnumerable<CompanyDTO>>> GetAllCompaniesAsync();
    Task<ApiResponse<CompanyDTO>> GetRandomCompanyAsync();
    Task<ApiResponse<CompanyDTO>> GetCompanyByIdAsync(Guid companyId);
}