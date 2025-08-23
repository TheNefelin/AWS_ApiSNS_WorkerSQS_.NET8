using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services;
using Microsoft.AspNetCore.Mvc;

namespace AWS_SNS_WebApi.Controllers;

[Route("api/donations")]
[ApiController]
public class DonationsController : ControllerBase
{
    private readonly ILogger<DonationsController> _logger;
    private readonly AppDbContext _context;
    private readonly SnsService _snsService;

    public DonationsController(ILogger<DonationsController> logger, AppDbContext context, SnsService snsService)
    {
        _logger = logger;
        _context = context;
        _snsService = snsService;
    }

    [HttpGet("reason")]
    public async Task<ActionResult<IEnumerable<FormDonation>>> GetReason()
    {
        return Ok(new { Message = "Ayuda a construir un mañana más brillante (y más controlado). ¡Dona para financiar la próxima generación de cibernética de consumo, robots de seguridad mejorados y bioingeniería para un mundo más limpio!" });
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetCompanies()
    {
        var companies = await _context.GetCompanies();
        return Ok(companies);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
    {
        var products = await _context.GetProducts();
        return Ok(products);
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<ApiResponse<string>>> Subscribe(FormSubscribe formSubscribe)
    {
        var apiResponse = await _snsService.Subscribe(formSubscribe);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("donate")]
    public async Task<IActionResult> Donate(FormDonation formDonation)
    {
        try
        {
            var message = await _snsService.PublishIndividualDonation(formDonation);
            return Ok(new { message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error while subscribing.");
        }
    }

    [HttpPost("notification")]
    public async Task<IActionResult> SendMassNotification([FromBody] string message)
    {
        try
        {
            await _snsService.PublishMassiveNotification(message);
            return Ok(new { message = "Mass notification sent successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending mass notification.");
            return StatusCode(500, "Internal server error while sending mass notification.");
        }
    }
}
