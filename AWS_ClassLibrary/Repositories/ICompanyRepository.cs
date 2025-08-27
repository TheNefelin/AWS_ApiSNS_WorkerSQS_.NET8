using AWS_ClassLibrary.DTOs;

namespace AWS_ClassLibrary.Repositories;

public interface ICompanyRepository
{
    Task<IEnumerable<CompanyDTO>> GetAllCompaniesAsync();
    Task<CompanyDTO?> GetRandomCompanyAsync();
    Task<CompanyDTO?> GetCompanyByIdAsync(Guid companyId);
}