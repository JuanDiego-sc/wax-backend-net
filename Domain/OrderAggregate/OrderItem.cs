using Domain.Entities;
namespace Domain.OrderAggregate;

public class OrderItem : BaseEntity
{
    public required ProductOrderItem ItemOrdered { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }
}
