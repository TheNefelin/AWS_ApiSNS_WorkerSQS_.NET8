using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.Entities;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
    public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
    {
        var companies = await _context.GetCompanies();
        return Ok(companies);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.GetProducts();
        return Ok(products);
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(FormSubscribe formSubscribe)
    {
        try
        {
            var message = await _snsService.Subscribe(formSubscribe);
            return Ok(new { message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing email: {Email}", formSubscribe.Email);
            return StatusCode(500, "Internal server error while subscribing.");
        }
    }

    [HttpPost("donate")]
    public async Task<IActionResult> Donate(FormDonation donation)
    {
        try
        {
            return Ok(new { message = "Donation recorded successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error while subscribing.");
        }
    }
}
