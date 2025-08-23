using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWS_ClassLibrary.Models;

public class Product
{
    [Key]
    [Column("product_id")]
    public Guid Product_id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("price")]
    public int Price { get; set; }
}
