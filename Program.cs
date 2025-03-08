using Microsoft.ApplicationInsights;
using NotificationService;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IConnection>(provider =>
//{
//    var telemetryClient = provider.GetRequiredService<TelemetryClient>(); // Inject TelemetryClient
//    try
//    {
//        var factory = new ConnectionFactory() { HostName = builder.Configuration["RabbitMQ:HostName"] };
//        return factory.CreateConnection();
//    }
//    catch (Exception ex)
//    {
//        // Log the exception using Application Insights (via injected TelemetryClient)
//        telemetryClient.TrackException(ex);

//        // Log locally using the logger
//        var logger = provider.GetRequiredService<ILogger<Program>>(); // Get ILogger from DI container
//        logger.LogError(ex, "RabbitMQ connection failed.");

//        // Rethrow the exception to fail the application startup
//        throw new InvalidOperationException("Failed to create RabbitMQ connection.", ex);
//    }
//});

builder.Services.AddSingleton<MessageRepository>();
//builder.Services.AddHostedService<RabbitMqService>();
//builder.Services.AddHostedService<Consumer2Service>();
//builder.Services.AddSingleton<IQueueChannel, RabbitMqChannel>();
//builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
