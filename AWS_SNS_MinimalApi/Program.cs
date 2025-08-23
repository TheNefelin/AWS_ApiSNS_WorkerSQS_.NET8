using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS_ClassLibrary;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure AWS region and SNS topic ARN
var AWS_REGION = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var AWS_SNS_TOPIC_ARN = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN") ?? "arnarn:aws:sns:us-east-1:527237860774:test-sns";

// Validate the topic ARN
if (string.IsNullOrEmpty(AWS_REGION) || string.IsNullOrEmpty(AWS_SNS_TOPIC_ARN))
{
    throw new InvalidOperationException("Faltan las variables de entorno AWS_REGION o SNS_TOPIC_ARN.");
}

// Register the Amazon SNS client
builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
{
    return new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(AWS_REGION));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Check if the API is running
app.MapGet("/status", () => Results.Ok(new { message = "API Corriendo" }));

// 4. Define a simple model for the PedidoCreado event
app.MapPost("/api/pedidos", async (Order pedido, IAmazonSimpleNotificationService sns) =>
{
    try
    {
        var json = System.Text.Json.JsonSerializer.Serialize(pedido);

        await sns.PublishAsync(new PublishRequest
        {
            TopicArn = AWS_SNS_TOPIC_ARN,
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

// Endpoint para suscribir un nuevo correo a un tema de SNS
app.MapPost("/api/suscripcion", async (string email, IAmazonSimpleNotificationService sns) =>
{
    try
    {
        if (string.IsNullOrEmpty(email))
        {
            return Results.BadRequest(new { message = "El correo no puede estar vacío." });
        }

        // 1. Buscar si el correo ya está suscrito o pendiente
        var subscriptionList = await sns.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
        {
            TopicArn = AWS_SNS_TOPIC_ARN
        });

        var existingSubscription = subscriptionList.Subscriptions
            .FirstOrDefault(s => s.Endpoint == email);

        if (existingSubscription != null)
        {
            // 2. Comprobar el estado de la suscripción
            if (existingSubscription.SubscriptionArn != "PendingConfirmation")
            {
                // La suscripción ya está confirmada
                return Results.Ok(new { message = "Ya estás suscrito.", status = "Confirmado" });
            }
            else
            {
                // La suscripción está pendiente de confirmación
                return Results.Ok(new { message = "Solicitud de suscripción ya enviada. Revisa tu correo para confirmar.", status = "Pendiente" });
            }
        }

        // 3. Si no existe, crear una nueva suscripción
        var subscribeRequest = new SubscribeRequest
        {
            TopicArn = AWS_SNS_TOPIC_ARN,
            Protocol = "email",
            Endpoint = email
        };

        var response = await sns.SubscribeAsync(subscribeRequest);

        return Results.Created($"/api/suscripcion/{email}", new { message = "Suscripción iniciada. Revisa tu correo para confirmar.", status = "Pendiente" });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error interno en la API"
        );
    }
});

// Endpoint para obtener el estado de una suscripción
app.MapGet("/api/suscripcion/estado/{email}", async (string email, IAmazonSimpleNotificationService sns) =>
{
    try
    {
        var subscriptionList = await sns.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
        {
            TopicArn = AWS_SNS_TOPIC_ARN
        });

        var subscription = subscriptionList.Subscriptions.FirstOrDefault(s => s.Endpoint == email);

        if (subscription == null)
        {
            return Results.NotFound(new { message = "El correo no está suscrito.", status = "No Encontrado" });
        }

        var status = (subscription.SubscriptionArn != "PendingConfirmation") ? "Confirmado" : "Pendiente";

        return Results.Ok(new { message = $"El estado de la suscripción es: {status}", status = status });
    }
    catch (Exception ex)
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
