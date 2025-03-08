using NotificationService;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IConnection>(provider =>
{
    var factory = new ConnectionFactory() { HostName = builder.Configuration["RabbitMQ:HostName"] }; 
    return factory.CreateConnection();
});

builder.Services.AddSingleton<MessageRepository>();
builder.Services.AddHostedService<RabbitMqService>();
builder.Services.AddHostedService<Consumer2Service>();
builder.Services.AddSingleton<IQueueChannel, RabbitMqChannel>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

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
