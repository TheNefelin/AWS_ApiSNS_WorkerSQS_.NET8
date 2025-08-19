using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure AWS region and SNS topic ARN
var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var topicArn = Environment.GetEnvironmentVariable("TOPIC_ARN") ?? "arn:aws:sns:us-east-1:123:ecomexpress-sns";

if (string.IsNullOrEmpty(topicArn))
{
    throw new InvalidOperationException("El ARN del topic SNS no está configurado. Por favor, define TOPIC_ARN en tu configuración.");
}

// 2. Register the Amazon SNS client
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(region)));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Define a simple model for the PedidoCreado event
app.MapPost("/pedidos", async (PedidoCreado pedido, IAmazonSimpleNotificationService sns) =>
{
    var json = System.Text.Json.JsonSerializer.Serialize(pedido);

    await sns.PublishAsync(new PublishRequest
    {
        TopicArn = topicArn,
        Message = json,
        Subject = "Nuevo Pedido"
    });

    return Results.Ok(new { status = "Publicado en SNS", pedido.PedidoId });
});

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

// 4. Define the PedidoCreado record
public record PedidoCreado(Guid PedidoId, Guid ClienteId, DateTimeOffset Fecha, decimal Total);
