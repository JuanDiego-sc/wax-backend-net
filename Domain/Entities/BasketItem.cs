using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("BasketItems")]
public class BasketItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Quantity { get; set; }
    //navigation properties
    public string ProductId { get; set; } = "";
    public Product Product { get; set; } = null!;
    public string BasketId { get; set; } = "";
    public Basket Basket { get; set; } = null!;

}