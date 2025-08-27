using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Entities;

public class Donation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    public int Amount { get; set; }

    public int Total { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
