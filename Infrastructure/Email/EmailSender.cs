using System;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Resend;

namespace Infrastructure.Email;

public class EmailSender(ResendClient resendClient) : IEmailSender<User>
{
    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public async Task SendEmailAsync(string email, string subject, string htmlBody)
    {
        var message = new EmailMessage
        {
            From = "test@resend.dev",
            Subject= subject,
            HtmlBody = htmlBody,
        };

        message.To.Add(email);
        await resendClient.EmailSendAsync(message);
        await Task.CompletedTask;
    }
}
