using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Models;

public class FormEmail
{
    [EmailAddress]
    public required string Email { get; set; }
}
