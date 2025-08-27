using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Entities;

public class DailyStatistics
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int TotalDonations { get; set; }

    public int TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
