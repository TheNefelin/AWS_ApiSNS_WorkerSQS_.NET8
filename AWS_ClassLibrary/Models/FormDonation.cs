using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Models;

public class FormDonation
{
    [EmailAddress]
    public required string Email { get; set; }

    public required int Amount { get; set; }
}
