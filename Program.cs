using NotificationService;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHostedService<RabbitMqService>();
builder.Services.AddHostedService<Consumer2Service>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MessageRepository>();

builder.Services.AddSingleton<IConnection>(provider =>
{
    var factory = new ConnectionFactory() { HostName = builder.Configuration["RabbitMQ:HostName"] }; // Assume you have it in your appsettings.json
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IQueueChannel, RabbitMqChannel>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
