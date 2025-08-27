using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;

namespace AWS_ClassLibrary.Services.Application;

public interface IPdfGenerationService
{
    Task<Stream> GenerateInvoicePdfAsync(FormDonation donation, CompanyDTO company, List<ProductDTO> products, Stream companyImageStream);
}
