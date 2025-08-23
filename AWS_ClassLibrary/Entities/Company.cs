using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWS_ClassLibrary.Entities;

public class Company
{
    [Key]
    [Column("company_id")]
    public Guid Company_id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("email")]
    public string Email { get; set; }

    [Column("img")]
    public string? Img { get; set; }
}
