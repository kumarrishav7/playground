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

app.UseCors(options =>
    options.WithOrigins("http://localhost:4200", "https://ambitious-beach-032f22500.6.azurestaticapps.net")
            .AllowAnyMethod()
            .AllowAnyHeader());
app.Run();
