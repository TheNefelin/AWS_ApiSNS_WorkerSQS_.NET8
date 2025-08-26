using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services;

public class Mediator
{
    private readonly ILogger<Mediator> _logger;
    private readonly AppDbContext _context;
    private readonly S3Service _s3Service;
    private readonly SnsService _snsService;
    private readonly InvoiceGenerator _invoiceGenerator;

    public Mediator(ILogger<Mediator> logger, AppDbContext context, S3Service s3Service, SnsService snsService, InvoiceGenerator invoiceGenerator)
    {
        _logger = logger;
        _context = context;
        _s3Service = s3Service;
        _snsService = snsService;
        _invoiceGenerator = invoiceGenerator;
    }

    public async Task<ApiResponse<string>> GetReason()
    {
        return new ApiResponse<string>
        {
            Success = true,
            StatusCode = 200,
            Message = "Reason retrieved successfully.",
            Data = "Ayuda a construir un mañana más brillante (y más controlado). ¡Dona para financiar la próxima generación de cibernética de consumo, robots de seguridad mejorados y bioingeniería para un mundo más limpio!"
        };
    }

    public async Task<ApiResponse<IEnumerable<CompanyDTO>>> GetCompanies()
    {
        try
        {
            var dto = await _context.GetCompanies();

            return new ApiResponse<IEnumerable<CompanyDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Companies retrieved successfully.",
                Data = dto
            };
        }
        catch (Exception ex) { 
            return new ApiResponse<IEnumerable<CompanyDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "An error occurred while retrieving companies.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<CompanyDTO>> GetCompanyRandom()
    {
        try
        {
            var entity = await _context.GetCompanyRandom();

            if (entity == null)
            {
                return new ApiResponse<CompanyDTO>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "No companies found.",
                    Data = null
                };
            }

            return new ApiResponse<CompanyDTO>
            {
                Success = true,
                StatusCode = 200,
                Message = "Random company retrieved successfully.",
                Data = new CompanyDTO
                {
                    Company_id = entity.Company_id,
                    Name = entity.Name,
                    Email = entity.Email,
                    Img = entity.Img
                }
            };
        }
        catch
        {
            return new ApiResponse<CompanyDTO>
            {
                Success = false,
                StatusCode = 500,
                Message = "An error occurred while retrieving a random company.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDTO>>> GetProducts()
    {
        try
        {
            var dto = await _context.GetProducts();

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Products retrieved successfully.",
                Data = dto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "An error occurred while retrieving products.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDTO>>> GetProdctsByAmount(int amount)
    {
        try
        {
            var entities = await _context.GetProdctsByAmount(amount);

            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = true,
                StatusCode = 200,
                Message = "Products retrieved successfully.",
                Data = entities
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<ProductDTO>>
            {
                Success = false,
                StatusCode = 500,
                Message = "An error occurred while retrieving products.",
                Data = null
            };
        }
    }

    public async Task<ApiResponse<string>> Donation(FormDonation formDonation)
    {
        try
        {
            if (formDonation.Amount < 1 || formDonation.Amount > 3)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "La cantidad de donacion debe ser entre 1 y 3",
                    Data = formDonation.Amount.ToString()
                };
            }

            var company = await _context.GetCompanyRandom();
            var products = await _context.GetProdctsByAmount(formDonation.Amount);

            if (company == null || !products.Any())
            {
                _logger.LogError("[DonationsController] - Health check failed: No company or products found.");
                throw new Exception("Health check failed: No company or products found.");
            }

            using var imgStream = await _s3Service.GetFileStreamFromBucket(company.Img!);

            var pdfStream = _invoiceGenerator.CreateStreamPdf(formDonation, company, products.ToList(), imgStream);
            var fileName = await _s3Service.SavePdfToBucket(pdfStream);
            string downloadLink = _s3Service.GeneratePreSignedUrl(fileName, TimeSpan.FromDays(7));

            var donationStatus = await _snsService.PublishIndividualDonation(formDonation.Email, products.Sum(p => p.Price), downloadLink);

            if (donationStatus !=  null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Error sending donation email.",
                    Data = donationStatus
                };
            }

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = "Gracias por tu Donacion, recibirás un correo con la factura y el detalle",
                Data = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating or saving PDF.");

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = $"An error occurred while generating or saving the PDF. {ex}",
                Data = ex.ToString()
            };
        }
    }
}
