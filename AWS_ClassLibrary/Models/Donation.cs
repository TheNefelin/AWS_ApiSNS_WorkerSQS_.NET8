using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Models;

public class Donation
{
    [EmailAddress]
    public required string Email { get; set; }

    public required int amount { get; set; }
}
