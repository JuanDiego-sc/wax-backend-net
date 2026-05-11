namespace Domain.ProductAggregate;

public enum CustomProductStatus
{
    PendingQuotation = 0,
    AwaitingAdminReview = 1,
    AwaitingCustomerReview = 2,
    Approved = 3,
    Rejected = 4,
    AddedToBasket = 5
}