using Application.Notifications.Requests;

namespace Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default);

}