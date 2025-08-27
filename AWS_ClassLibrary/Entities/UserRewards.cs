using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Entities;

public class UserRewards
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    public int TotalPoints { get; set; }

    public int Level { get; set; } = 1;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
