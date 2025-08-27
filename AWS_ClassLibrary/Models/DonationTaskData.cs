using AWS_ClassLibrary.DTOs;

namespace AWS_ClassLibrary.Models;

public class DonationTaskData
{
    public required string Email { get; set; }
    public required int Amount { get; set; }
    public required CompanyDTO Company { get; set; }
    public required List<ProductDTO> Products { get; set; }
}
