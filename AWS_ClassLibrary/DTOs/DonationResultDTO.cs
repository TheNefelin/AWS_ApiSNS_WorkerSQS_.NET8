namespace AWS_ClassLibrary.DTOs;

public class DonationResultDTO
{
    public string Email { get; set; } = string.Empty;
    public int Amount { get; set; }
    public CompanyDTO Company { get; set; } = new();
    public List<ProductDTO> Products { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
