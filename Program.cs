using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using NotificationService;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});
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
        var logger = provider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "RabbitMQ connection failed.");
        throw new InvalidOperationException("Failed to create RabbitMQ connection.", ex);
    }
});

builder.Services.AddRateLimiter(rateLimiter =>
{
    rateLimiter.AddSlidingWindowLimiter("sliding", options =>
    {
        options.Window = TimeSpan.FromSeconds(60);
        options.SegmentsPerWindow = 6;
        options.PermitLimit = 8000;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddSingleton<MessageRepository>();
builder.Services.AddSingleton<IQueueChannel, RabbitMqChannel>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

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
