using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.OrderAggregate;

[Table("OrderItems")]
public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required ProductOrderItem ItemOrdered { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }
}
