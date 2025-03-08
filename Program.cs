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
        var factory = new ConnectionFactory() { HostName = "20.116.232.7",
                                                UserName = "kumarrishav7",
                                                Password = "Llbgdbg@123",
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

builder.Services.AddSingleton<MessageRepository>();
builder.Services.AddHostedService<RabbitMqService>();
builder.Services.AddHostedService<Consumer2Service>();
builder.Services.AddSingleton<IQueueChannel, RabbitMqChannel>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
