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
    public async Task<ActionResult<ApiResponse<string>>> Subscribe(FormEmail formEmail)
    {
        var apiResponse = await _snsService.Subscribe(formEmail);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("unsubscribe")]
    public async Task<ActionResult<ApiResponse<string>>> Unsubscribe(FormEmail formEmail)
    {
        var apiResponse = await _snsService.Unsubscribe(formEmail);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("donate")]
    public async Task<ActionResult<ApiResponse<string>>> Donate(FormDonation formDonation)
    {
        var apiResponse = await _snsService.PublishIndividualDonation(formDonation);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }

    [HttpPost("notification")]
    public async Task<ActionResult<ApiResponse<string>>> SendMassNotification(string message)
    {
        var apiResponse = await _snsService.PublishMassiveNotification(message);
        return StatusCode(apiResponse.StatusCode, apiResponse);
    }
}
