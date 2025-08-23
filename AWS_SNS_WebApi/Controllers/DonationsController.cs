using Amazon.SimpleNotificationService.Util;
using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AWS_SNS_WebApi.Controllers;

[Route("api/donations")]
[ApiController]
public class DonationsController : ControllerBase
{
    private readonly ILogger<DonationsController> _logger;
    private readonly AppDbContext _context;

    public DonationsController(ILogger<DonationsController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("reason")]
    public async Task<ActionResult<IEnumerable<Donation>>> GetReason()
    {
        return Ok(new { Message = "Ayuda a construir un mañana más brillante (y más controlado). ¡Dona para financiar la próxima generación de cibernética de consumo, robots de seguridad mejorados y bioingeniería para un mundo más limpio!" });
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        var companies = await _context.companies.ToListAsync();
        return Ok(companies);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.products.ToListAsync();
        return Ok(products);
    }

    [HttpPost("donate")]
    public async Task<IActionResult> Donate(Donation donation)
    {
        return Ok(new { message = "Donation recorded successfully." });
    }
}
