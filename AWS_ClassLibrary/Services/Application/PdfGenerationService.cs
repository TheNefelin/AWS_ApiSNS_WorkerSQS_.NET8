using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services.Utilities;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services.Application;

public class PdfGenerationService : IPdfGenerationService
{
    private readonly ILogger<PdfGenerationService> _logger;
    private readonly InvoiceGenerator _invoiceGenerator;

    public PdfGenerationService(ILogger<PdfGenerationService> logger, InvoiceGenerator invoiceGenerator)
    {
        _logger = logger;
        _invoiceGenerator = invoiceGenerator;
    }

    public async Task<Stream> GenerateInvoicePdfAsync(FormDonation donation, CompanyDTO company, List<ProductDTO> products, Stream companyImageStream)
    {
        try
        {
            _logger.LogInformation("Generando PDF para donación - Email: {Email}, Productos: {ProductCount}",
                donation.Email, products.Count);

            var pdfStream = _invoiceGenerator.CreateStreamPdf(donation, company, products, companyImageStream);

            _logger.LogInformation("PDF generado exitosamente para: {Email}", donation.Email);
            return pdfStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF para donación - Email: {Email}", donation.Email);
            throw;
        }
    }
}

