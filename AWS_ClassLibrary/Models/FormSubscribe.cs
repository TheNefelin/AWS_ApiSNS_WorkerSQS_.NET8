using System.ComponentModel.DataAnnotations;

namespace AWS_ClassLibrary.Models;

public class FormSubscribe
{
    [EmailAddress]
    public required string Email { get; set; }
}
