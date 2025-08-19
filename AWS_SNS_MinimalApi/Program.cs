using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Register the Amazon SNS client
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Define a simple model for the PedidoCreado event
app.MapPost("/pedidos", async (PedidoCreado pedido, IAmazonSimpleNotificationService sns) =>
{
    var json = System.Text.Json.JsonSerializer.Serialize(pedido);

    await sns.PublishAsync(new PublishRequest
    {
        TopicArn = "arn:aws:sns:REGION:ACCOUNT_ID:pedidos-eventos",
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

// 3. Define the PedidoCreado record
public record PedidoCreado(string PedidoId, DateTimeOffset Fecha, string ClienteId, decimal Total);
