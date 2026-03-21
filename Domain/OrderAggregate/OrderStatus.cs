namespace Domain.OrderAggregate;

public enum OrderStatus
{
    Pending,
    CustomOrder,
    Approved,
    Rejected,
    PaymentRecieved,
    PaymentFailed,
    PaymentMismatch
}
