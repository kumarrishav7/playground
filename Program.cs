using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using NotificationService;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnection>(provider =>
{
    try
    {
        var factory = new ConnectionFactory() { HostName = builder.Configuration["RabbitMQ:HostName"],
                                                UserName = builder.Configuration["RabbitMQ:UserName"],
                                                Password = builder.Configuration["RabbitMQ:Password"],
        };
        return factory.CreateConnection();
    }
    catch (Exception ex)
    {
        // Log locally using the logger
        var logger = provider.GetRequiredService<ILogger<Program>>(); // Get ILogger from DI container
        logger.LogError(ex, "RabbitMQ connection failed.");

        // Rethrow the exception to fail the application startup
        throw new InvalidOperationException("Failed to create RabbitMQ connection.", ex);
    }
});

builder.Services.AddRateLimiter(rateLimiter =>
{
    rateLimiter.AddSlidingWindowLimiter("sliding", options =>
    {
        options.Window = TimeSpan.FromSeconds(60);
        options.SegmentsPerWindow = 6;
        // Define the maximum number of requests allowed within the window
        options.PermitLimit = 8000;
        // Set the queue processing order for requests beyond the limit
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddSingleton<MessageRepository>();
//builder.Services.AddHostedService<RabbitMqService>();
//builder.Services.AddHostedService<Consumer2Service>();
builder.Services.AddSingleton<IQueueChannel, RabbitMqChannel>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseRateLimiter();

app.UseCors(options =>
    options.WithOrigins("http://localhost:4200", "https://ambitious-beach-032f22500.6.azurestaticapps.net")
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader());
app.Run();
