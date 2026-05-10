using Domain.Enumerators;

namespace Domain.ProductAggregate;

public class CustomProduct : Product
{
    public required string TaskId { get; set; }
    public required string GlbUrl { get; set; }
    public required string RawDescription { get; set; }
    public required string OwnerUserId { get; set; }
    public string? BasketId { get; set; }
    public CustomProductStatus Status { get; set; } = CustomProductStatus.PendingQuotation;
    public CustomProductDesign Design { get; set; } = null!;
    public List<PriceProposal> Proposals { get; set; } = [];
    public long? AgreedPrice { get; set; }
    public string? RejectionReason { get; set; }

    public override string Kind => ProductTypes.Custom;

    public PriceProposal RegisterSystemQuotation(long amount)
    {
        if (Status != CustomProductStatus.PendingQuotation)
            throw new InvalidOperationException("The automatic quotation is registered only one time");

        var proposal = new PriceProposal
        {
            CustomProductId = Id,
            Amount = amount,
            Source = ProposalSource.System
        };
        Proposals.Add(proposal);
        Status = CustomProductStatus.AwaitingAdminReview;
        Price = amount;
        return proposal;
    }

    public PriceProposal RegisterAdminProposal(long amount, string? comment)
    {
        if (Status != CustomProductStatus.AwaitingAdminReview)
            throw new InvalidOperationException("The admin can not propose in this state");

        var proposal = new PriceProposal
        {
            CustomProductId = Id,
            Amount = amount,
            Source = ProposalSource.Admin,
            Comment = comment
        };
        Proposals.Add(proposal);
        Status = CustomProductStatus.AwaitingCustomerReview;
        Price = amount;
        return proposal;
    }

    public PriceProposal RegisterCustomerProposal(long amount, string? comment)
    {
        if (Status != CustomProductStatus.AwaitingCustomerReview)
            throw new InvalidOperationException("The client can not propose in this state");

        var proposal = new PriceProposal
        {
            CustomProductId = Id,
            Amount = amount,
            Source = ProposalSource.Customer,
            Comment = comment
        };
        Proposals.Add(proposal);
        Status = CustomProductStatus.AwaitingAdminReview;
        Price = amount;
        return proposal;
    }

    public void Approve(ProposalSource approver)
    {
        var last = Proposals.LastOrDefault()
            ?? throw new InvalidOperationException("There is no an active proposal");

        if (approver == ProposalSource.System)
            throw new InvalidOperationException("The system not approve");

        var awaitingApprover = approver switch
        {
            ProposalSource.Admin => Status == CustomProductStatus.AwaitingAdminReview
                                    && last.Source == ProposalSource.Customer,
            ProposalSource.Customer => Status == CustomProductStatus.AwaitingCustomerReview
                                        && last.Source is ProposalSource.Admin or ProposalSource.System,
            _ => false
        };

        if (!awaitingApprover)
            throw new InvalidOperationException("can not approve in this state");

        last.IsAccepted = true;
        AgreedPrice = last.Amount;
        Price = last.Amount;
        Status = CustomProductStatus.Approved;
    }

    public void Reject(string reason)
    {
        if (Status is CustomProductStatus.Approved or CustomProductStatus.AddedToBasket)
            throw new InvalidOperationException("can not reject a product approved");
        Status = CustomProductStatus.Rejected;
        RejectionReason = reason;
    }

    public void MarkAddedToBasket()
    {
        if (Status != CustomProductStatus.Approved)
            throw new InvalidOperationException("Only approved products are into the basket");
        Status = CustomProductStatus.AddedToBasket;
    }
}