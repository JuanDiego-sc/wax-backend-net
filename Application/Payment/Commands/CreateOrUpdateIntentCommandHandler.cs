using Application.Basket.DTOs;
using Application.Basket.Extensions;
using Application.Core.Validations;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Payment.Commands;

public class CreateOrUpdateIntentCommandHandler(
    IBasketRepository basketRepository,
    IUnitOfWork unitOfWork,
    IPaymentService paymentService)
    : IRequestHandler<CreateOrUpdateIntentCommand, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(CreateOrUpdateIntentCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetBasketWithItemsAsync(request.BasketId, cancellationToken);
        if (basket == null) return Result<BasketDto>.Failure("Basket not found");

        var intent = await paymentService.CreateOrUpdatePaymentIntent(basket);
        if (intent == null) return Result<BasketDto>.Failure("Problem creating payment intent");
        
        basket.PaymentIntentId ??= intent.PaymentIntentId;
        basket.ClientSecret ??= intent.ClientSecret;
        
        if (unitOfWork.HasChanges())
        {
            var result = await unitOfWork.CompleteAsync(cancellationToken);
            if (!result) return Result<BasketDto>.Failure("Problem updating basket with intent");
        }

        return Result<BasketDto>.Success(basket.ToDto());
    }
}
