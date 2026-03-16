using Application.Basket.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Cookies;

public class BasketCookieService(IHttpContextAccessor contextAccessor) : IBasketProvider
{
    private const string BasketIdCookieName = "basketId";
    
    public string? GetBasketId()
    {
        return contextAccessor.HttpContext?.Request.Cookies[BasketIdCookieName];
    }

    public void SetBasketId(string basketId)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(30)
        };
        
        contextAccessor.HttpContext?.Response.Cookies.Append(BasketIdCookieName, basketId, cookieOptions);
    }
}