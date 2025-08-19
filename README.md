# AWS SNS Minimal API + AWS SQS Worker Service .NET8

### Requerimientos
- AWS_SNS_MinimalApi
```
AWSSDK.SimpleNotificationService
```

- AWS_SQS_WorkerService
```	
```

### AWS_SNS_MinimalApi Program.cs
```csharp
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

var builder = WebApplication.CreateBuilder(args);

// Registrar AWS SNS client
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1));

var app = builder.Build();

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

app.Run();

public record PedidoCreado(string PedidoId, DateTimeOffset Fecha, string ClienteId, decimal Total);
```