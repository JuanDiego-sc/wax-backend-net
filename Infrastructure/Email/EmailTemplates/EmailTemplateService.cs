using Application.Notifications.Requests;

namespace Infrastructure.Email.EmailTemplates;

public class EmailTemplateService
{
    public (string Subject, string HtmlBody) GetTemplate(EmailRequest request)
    {
        return request switch
        {
            AccountConfirmationEmailRequest r => GetAccountConfirmationTemplate(r),
            PasswordResetEmailRequest r => GetPasswordResetTemplate(r),
            OrderStatusChangedEmailRequest r => GetOrderStatusChangedTemplate(r),
            PaymentConfirmedEmailRequest r => GetPaymentConfirmedTemplate(r),
            SupportTicketCreatedEmailRequest r => GetSupportTicketCreatedTemplate(r),
            SupportTicketUpdatedEmailRequest r => GetSupportTicketUpdatedTemplate(r),
            _ => throw new ArgumentException($"No template for {request.EmailType}")
        };
    }

    private static (string, string) GetAccountConfirmationTemplate(AccountConfirmationEmailRequest request)
    {
        var subject = "Confirma tu cuenta";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Bienvenido a WAX</h2>
                <p>Hola {request.ToName},</p>
                <p>Gracias por registrarte. Por favor confirma tu cuenta haciendo clic en el siguiente enlace:</p>
                <p><a href="{request.ConfirmationLink}" style="background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Confirmar cuenta</a></p>
                <p>Si no creaste esta cuenta, ignora este email.</p>
            </body>
            </html>
            """;
        return (subject, body);
    }

    private static (string, string) GetPasswordResetTemplate(PasswordResetEmailRequest request)
    {
        var subject = "Restablecer contrasena";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Restablecer contrasena</h2>
                <p>Hola {request.ToName},</p>
                <p>Recibimos una solicitud para restablecer tu contrasena.</p>
                <p>Tu codigo de verificacion es: <strong>{request.ResetCode}</strong></p>
                <p>O haz clic aqui: <a href="{request.ResetLink}">Restablecer contrasena</a></p>
                <p>Este codigo expira en 15 minutos.</p>
            </body>
            </html>
            """;
        return (subject, body);
    }

    private static (string, string) GetOrderStatusChangedTemplate(OrderStatusChangedEmailRequest request)
    {
        var subject = $"Tu orden #{request.OrderNumber[..8]} ha cambiado de estado";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Actualizacion de tu orden</h2>
                <p>Hola {request.ToName},</p>
                <p>Tu orden <strong>#{request.OrderNumber[..8]}</strong> ha cambiado de estado:</p>
                <p><strong>{request.OldStatus}</strong> -> <strong>{request.NewStatus}</strong></p>
                <p>Total de la orden: ${request.Total / 100m:F2}</p>
            </body>
            </html>
            """;
        return (subject, body);
    }

    private static (string, string) GetPaymentConfirmedTemplate(PaymentConfirmedEmailRequest request)
    {
        var subject = $"Pago confirmado - Orden #{request.OrderNumber[..8]}";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Pago recibido</h2>
                <p>Hola {request.ToName},</p>
                <p>Hemos recibido tu pago correctamente.</p>
                <p><strong>Orden:</strong> #{request.OrderNumber[..8]}</p>
                <p><strong>Monto:</strong> ${request.TotalAmount / 100m:F2}</p>
            </body>
            </html>
            """;
        return (subject, body);
    }

    private static (string, string) GetSupportTicketCreatedTemplate(SupportTicketCreatedEmailRequest request)
    {
        var subject = $"Ticket de soporte creado - #{request.OrderNumber[..8]}";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Ticket de soporte creado</h2>
                <p>Hola {request.ToName},</p>
                <p>Hemos recibido tu solicitud de soporte.</p>
                <p><strong>Ticket:</strong> #{request.OrderNumber[..8]}</p>
                <p><strong>Asunto:</strong> {request.Subject}</p>
                <p>Te responderemos lo antes posible.</p>
            </body>
            </html>
            """;
        return (subject, body);
    }

    private static (string, string) GetSupportTicketUpdatedTemplate(SupportTicketUpdatedEmailRequest request)
    {
        var subject = $"Actualizacion de ticket - #{request.OrderNumber[..8]}";
        var body = $"""
            <html>
            <body style="font-family: Arial, sans-serif; padding: 20px;">
                <h2>Actualizacion de ticket</h2>
                <p>Hola {request.ToName},</p>
                <p>Tu ticket de soporte ha sido actualizado.</p>
                <p><strong>Ticket:</strong> #{request.OrderNumber[..8]}</p>
                <p><strong>Asunto:</strong> {request.Subject}</p>
                <p><strong>Nuevo estado:</strong> {request.NewStatus}</p>
            </body>
            </html>
            """;
        return (subject, body);
    }
}