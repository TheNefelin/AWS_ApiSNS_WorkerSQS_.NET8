using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services.Infrastructure;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services.Application;

public class DonationService : IDonationService
{
    private readonly ILogger<DonationService> _logger;
    private readonly ICompanyService _companyService;
    private readonly IProductService _productService;
    private readonly INotificationService _notificationService;
    private readonly IPdfGenerationService _pdfGenerationService;
    private readonly IAwsS3Service _s3Service;

    public DonationService(
        ILogger<DonationService> logger,
        ICompanyService companyService,
        IProductService productService,
        INotificationService notificationService,
        IPdfGenerationService pdfGenerationService,
        IAwsS3Service s3Service)
    {
        _logger = logger;
        _companyService = companyService;
        _productService = productService;
        _notificationService = notificationService;
        _pdfGenerationService = pdfGenerationService;
        _s3Service = s3Service;
    }

    public async Task<ApiResponse<string>> ProcessDonationRequestAsync(FormDonation donation)
    {
        try
        {
            _logger.LogInformation("Iniciando procesamiento de donación para: {Email} con cantidad: {Amount}",
                donation.Email, donation.Amount);

            // Validar cantidad de donación
            if (donation.Amount < 1 || donation.Amount > 3)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "La cantidad de donación debe ser entre 1 y 3",
                    Data = donation.Amount.ToString()
                };
            }

            // Obtener empresa y productos (operaciones rápidas para la API)
            var companyResponse = await _companyService.GetRandomCompanyAsync();
            var productsResponse = await _productService.GetRandomProductsAsync(donation.Amount);

            if (!companyResponse.Success || companyResponse.Data == null)
            {
                _logger.LogError("No se pudo obtener empresa para la donación");
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "No hay empresas disponibles en este momento.",
                    Data = null
                };
            }

            if (!productsResponse.Success || productsResponse.Data == null || !productsResponse.Data.Any())
            {
                _logger.LogError("No se pudieron obtener productos para la donación");
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "No hay productos disponibles en este momento.",
                    Data = null
                };
            }

            // Enviar tarea de procesamiento al Worker (operación asíncrona)
            await _notificationService.SendDonationProcessingTaskAsync(
                donation,
                companyResponse.Data,
                productsResponse.Data.ToList());

            // Respuesta inmediata al usuario
            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = "¡Gracias por tu donación! Recibirás un correo con la factura en los próximos minutos.",
                Data = "Procesando donación..."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando solicitud de donación para: {Email}", donation.Email);

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error procesando la donación. Intenta nuevamente.",
                Data = ex.Message
            };
        }
    }

    public async Task<DonationResultDTO> ProcessDonationBackgroundAsync(DonationProcessingTask task)
    {
        var result = new DonationResultDTO
        {
            Email = task.DonationData.Email,
            Amount = task.DonationData.Amount,
            Company = task.DonationData.Company,
            Products = task.DonationData.Products,
            TotalAmount = task.DonationData.Products.Sum(p => p.Price),
            Status = "Processing"
        };

        try
        {
            _logger.LogInformation("Procesando donación en background para: {Email} | Cantidad: {Amount} | Total: ${Total}",
                task.DonationData.Email,
                task.DonationData.Amount,
                result.TotalAmount);

            // 1. OBTENER IMAGEN DE S3
            using var imgStream = await _s3Service.GetFileStreamFromBucketAsync(task.DonationData.Company.Img!);
            _logger.LogInformation("✅ Imagen obtenida de S3: {ImageName}", task.DonationData.Company.Img);

            // 2. GENERAR PDF
            var formDonation = new FormDonation
            {
                Email = task.DonationData.Email,
                Amount = task.DonationData.Amount
            };

            using var pdfStream = await _pdfGenerationService.GenerateInvoicePdfAsync(
                formDonation,
                task.DonationData.Company,
                task.DonationData.Products,
                imgStream);
            _logger.LogInformation("✅ PDF generado exitosamente");

            // 3. SUBIR PDF A S3
            var fileName = await _s3Service.SavePdfToBucketAsync(pdfStream);
            _logger.LogInformation("✅ PDF guardado en S3: {FileName}", fileName);

            // 4. GENERAR LINK DE DESCARGA
            string downloadLink = _s3Service.GeneratePreSignedUrl(fileName, TimeSpan.FromDays(7));
            _logger.LogInformation("✅ Link de descarga generado");

            // 5. ENVIAR EMAIL
            await _notificationService.SendDonationReceiptAsync(
                task.DonationData.Email,
                (int)result.TotalAmount,
                downloadLink);
            _logger.LogInformation("✅ Email enviado exitosamente a: {Email}", task.DonationData.Email);

            result.Status = "Completed";
            _logger.LogInformation("🎉 Donación procesada completamente para: {Email}", task.DonationData.Email);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando donación para: {Email}", task.DonationData.Email);

            result.Status = "Failed";
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<ApiResponse<string>> GetDonationReasonAsync()
    {
        return new ApiResponse<string>
        {
            Success = true,
            StatusCode = 200,
            Message = "Razón de donación obtenida exitosamente.",
            Data = "Ayuda a construir un mañana más brillante (y más controlado). ¡Dona para financiar la próxima generación de cibernética de consumo, robots de seguridad mejorados y bioingeniería para un mundo más limpio!"
        };
    }
}
