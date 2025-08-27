using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services.Application;

namespace AWS_SNS_WebApi.Mediators;

public class ApiMediator : IApiMediator
{
    private readonly ILogger<ApiMediator> _logger;
    private readonly IDonationService _donationService;
    private readonly ICompanyService _companyService;
    private readonly IProductService _productService;
    private readonly INotificationService _notificationService;

    public ApiMediator(
        ILogger<ApiMediator> logger,
        IDonationService donationService,
        ICompanyService companyService,
        IProductService productService,
        INotificationService notificationService)
    {
        _logger = logger;
        _donationService = donationService;
        _companyService = companyService;
        _productService = productService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<string>> GetDonationReasonAsync()
    {
        _logger.LogInformation("Mediator - Obteniendo razón de donación");
        return await _donationService.GetDonationReasonAsync();
    }

    public async Task<ApiResponse<IEnumerable<CompanyDTO>>> GetCompaniesAsync()
    {
        _logger.LogInformation("Mediator - Obteniendo empresas");
        return await _companyService.GetAllCompaniesAsync();
    }

    public async Task<ApiResponse<IEnumerable<ProductDTO>>> GetProductsAsync()
    {
        _logger.LogInformation("Mediator - Obteniendo productos");
        return await _productService.GetAllProductsAsync();
    }

    public async Task<ApiResponse<string>> SubscribeEmailAsync(FormEmail formEmail)
    {
        _logger.LogInformation("Mediator - Procesando suscripción para: {Email}", formEmail.Email);
        return await _notificationService.SubscribeEmailAsync(formEmail);
    }

    public async Task<ApiResponse<string>> UnsubscribeEmailAsync(FormEmail formEmail)
    {
        _logger.LogInformation("Mediator - Procesando desuscripción para: {Email}", formEmail.Email);
        return await _notificationService.UnsubscribeEmailAsync(formEmail);
    }

    public async Task<ApiResponse<string>> ProcessDonationAsync(FormDonation formDonation)
    {
        _logger.LogInformation("Mediator - Procesando donación para: {Email}", formDonation.Email);
        return await _donationService.ProcessDonationRequestAsync(formDonation);
    }

    public async Task<ApiResponse<string>> SendMassiveNotificationAsync(string message)
    {
        _logger.LogInformation("Mediator - Enviando notificación masiva");
        return await _notificationService.SendMassiveNotificationAsync(message);
    }
}