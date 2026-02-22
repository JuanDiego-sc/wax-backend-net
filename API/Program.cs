using API.Middleware;
using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Email;
using Infrastructure.Photos;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Resend;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors();
builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblyContaining<Program>();
    x.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["ResendSettings:ApiToken"]!;
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddTransient<IEmailSender<User>, EmailSender>();

builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddTransient<ExceptionMiddleware>();
builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

//Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration
    .GetSection("CloudinarySettings"));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithOrigins(
    "http://localhost:5005", 
    "http://localhost:5006", 
    "http://localhost:5007", 
    "https://localhost:5005", 
    "https://localhost:5006", 
    "https://localhost:5007"));

app.MapControllers();
app.MapGroup("api").MapIdentityApi<User>();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    await context.Database.MigrateAsync();
    //await DbInitializer.SeedData(context, userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration and seeding data");
}

app.Run();
