using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_SNS_WebApi.Mediators;
using Microsoft.AspNetCore.Mvc;

namespace AWS_SNS_WebApi.Controllers;

[Route("api/donations")]
[ApiController]
public class DonationsController : ControllerBase
{
    private readonly ILogger<DonationsController> _logger;
    private readonly IApiMediator _mediator;

    public DonationsController(ILogger<DonationsController> logger, IApiMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("reason")]
    public async Task<ActionResult<ApiResponse<string>>> GetReason()
    {
        _logger.LogInformation("[DonationsController] - Retrieving donation reason.");
        var apiResponse = await _mediator.GetDonationReasonAsync();
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpGet("companies")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDTO>>>> GetCompanies()
    {
        _logger.LogInformation("[DonationsController] - Retrieving companies.");
        var apiResponse = await _mediator.GetCompaniesAsync();
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpGet("products")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDTO>>>> GetProducts()
    {
        _logger.LogInformation("[DonationsController] - Retrieving products.");
        var apiResponse = await _mediator.GetProductsAsync();
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<ApiResponse<string>>> Subscribe(FormEmail formEmail)
    {
        _logger.LogInformation("[DonationsController] - Subscribing email: {Email}", formEmail.Email);
        var apiResponse = await _mediator.SubscribeEmailAsync(formEmail);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("unsubscribe")]
    public async Task<ActionResult<ApiResponse<string>>> Unsubscribe(FormEmail formEmail)
    {
        _logger.LogInformation("[DonationsController] - Unsubscribing email: {Email}", formEmail.Email);
        var apiResponse = await _mediator.UnsubscribeEmailAsync(formEmail);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("donate")]
    public async Task<ActionResult<ApiResponse<string>>> Donate(FormDonation formDonation)
    {
        _logger.LogInformation("[DonationsController] - Processing donation from email: {Email} with amount: {Amount}",
            formDonation.Email, formDonation.Amount);

        var apiResponse = await _mediator.ProcessDonationAsync(formDonation);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("notification")]
    public async Task<ActionResult<ApiResponse<string>>> SendMassNotification(string message)
    {
        _logger.LogInformation("[DonationsController] - Sending mass notification with message: {Message}", message);
        var apiResponse = await _mediator.SendMassiveNotificationAsync(message);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }
}
