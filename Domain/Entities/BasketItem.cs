using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("BasketItems")]
public class BasketItem : BaseEntity
{
    public int Quantity { get; set; }

    #region Navigation Properties
    
    public string ProductId { get; set; } = "";
    public Product Product { get; set; } = null!;
    
    public string BasketId { get; set; } = "";
    public Basket Basket { get; set; } = null!;
    
    #endregion

}