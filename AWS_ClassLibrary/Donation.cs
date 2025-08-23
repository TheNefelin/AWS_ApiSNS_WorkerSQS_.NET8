using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary;

public class Donation
{
    [EmailAddress]
    public required string Email { get; set; }
    public required int Amount { get; set; }
}
