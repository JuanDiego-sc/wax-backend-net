namespace Domain.Entities;

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