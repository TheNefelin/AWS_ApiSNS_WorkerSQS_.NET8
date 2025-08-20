using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure AWS region and SNS topic ARN
var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN") ?? "arn:aws:sns:us-east-1:123:ecomexpress-sns-orders-events";

// 2. Validate the topic ARN
if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(topicArn))
{
    throw new InvalidOperationException("Faltan las variables de entorno AWS_REGION o SNS_TOPIC_ARN.");
}

// 2. Register the Amazon SNS client
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(region)));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Check if the API is running
app.MapGet("/status", () => Results.Ok(new { message = "API Corriendo" }));

// 4. Define a simple model for the PedidoCreado event
app.MapPost("/api/pedidos", async (PedidoCreado pedido, IAmazonSimpleNotificationService sns) =>
{
    try
    {
        var json = System.Text.Json.JsonSerializer.Serialize(pedido);

        await sns.PublishAsync(new PublishRequest
        {
            TopicArn = topicArn,
            Message = json,
            Subject = "Nuevo Pedido"
        });

        return Results.Ok(new { status = "Publicado en SNS", pedido.PedidoId });
    } catch (Exception ex)
    {
        return Results.Problem(
          detail: ex.Message,
          statusCode: 500,
          title: "Error interno en la API"
      );
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. Environments other than Development can still use Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./swagger/v1/swagger.json", "API v1"); // Usa "./" para compatibilidad
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration(); // Opcional: muestra tiempo de respuesta
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// 6. Define the PedidoCreado record
public record PedidoCreado(Guid PedidoId, Guid ClienteId, DateTimeOffset Fecha, decimal Total);
