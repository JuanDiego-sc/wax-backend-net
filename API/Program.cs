using API.Middleware;
using API.SignalR;
using Application.Basket.Commands;
using Application.Basket.Interfaces;
using Application.Core.Validations;
using Application.Interfaces;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Cookies;
using Infrastructure.Email;
using Infrastructure.Email.Adapters;
using Infrastructure.Email.EmailTemplates;
using Infrastructure.Email.Services;
using Infrastructure.Images;
using Infrastructure.Messaging;
using Infrastructure.Payments;
using Infrastructure.Repositories.ReadRepositories;
using Infrastructure.Repositories.WriteRepositories;
using Infrastructure.Security;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Resend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddDbContext<WriteDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("WriteConnection"));
});

builder.Services.AddDbContext<ReadDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReadConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddCors();
builder.Services.AddSignalR();

builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblyContaining<AddItemCommandHandler>();
    x.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddMassTransit(configuration =>
{
    configuration.AddConsumers(typeof(Infrastructure.AssemblyMarker).Assembly);
    configuration.AddEntityFrameworkOutbox<WriteDbContext>(options =>
    {
        options.UsePostgres();
        options.UseBusOutbox();
    });
    configuration.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMQ:Host"], host =>
        {
            host.Username(builder.Configuration["RabbitMQ:Username"]!);
            host.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
        
        config.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(5)));
        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddHttpClient("ResendClient");
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["EmailSettings:ApiToken"]!;
});
builder.Services.AddSingleton<IResend>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var options = sp.GetRequiredService<IOptions<ResendClientOptions>>();
    var httpClient = httpClientFactory.CreateClient("ResendClient");
    return new ResendClient(new OptionsSnapshotAdapter<ResendClientOptions>(options),  httpClient);
});
builder.Services.AddSingleton<EmailTemplateService>();
builder.Services.AddTransient<IEmailService, ResendEmailService>();
builder.Services.AddTransient<IEmailSender<User>, IdentityEmailSender>();

builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBasketProvider, BasketCookieService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>(); 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductReadRepository, ProductReadRepository>();
builder.Services.AddScoped<IOrderReadRepository, OrderReadRepository>();
builder.Services.AddScoped<ISupportTicketReadRepository, SupportTicketReadRepository>();

builder.Services.AddScoped<IEventPublisher, EventPublisher>();

builder.Services.AddIdentityApiEndpoints<User>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<WriteDbContext>();

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
app.MapHub<SupportHub>("/comments");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var writeContext = services.GetRequiredService<WriteDbContext>();
    var readContext = services.GetRequiredService<ReadDbContext>();
    await writeContext.Database.MigrateAsync();
    await readContext.Database.MigrateAsync();
    
    var initializer = new DbInitializer(
        services.GetRequiredService<UserManager<User>>(),
        services.GetRequiredService<RoleManager<IdentityRole>>(),
        services.GetRequiredService<IConfiguration>(),
        services.GetRequiredService<ILogger<DbInitializer>>());
    await initializer.InitializeAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration and seeding data");
}

await app.RunAsync();
