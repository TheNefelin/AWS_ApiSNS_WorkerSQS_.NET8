// See https://aka.ms/new-console-template for more information
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL") ?? "https://sqs.us-east-1.amazonaws.com/123/ecomexpress-sqs-process";

// Validación
if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(queueUrl))
{
    throw new InvalidOperationException("Faltan las variables de entorno AWS_REGION o QUEUE_URL.");
} else
{
    Console.WriteLine($"Usando región: {region}");
    Console.WriteLine($"Usando URL de cola: {queueUrl}");
}

// Crear cliente SQS usando la región de la variable
var sqs = new AmazonSQSClient(RegionEndpoint.GetBySystemName(region));

while (true)
{
    var resp = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
    {
        QueueUrl = queueUrl,
        MaxNumberOfMessages = 5,
        WaitTimeSeconds = 20,   // long polling
        VisibilityTimeout = 30
    });

    foreach (var msg in resp.Messages)
    {
        try
        {
            var pedido = JsonSerializer.Deserialize<PedidoCreado>(msg.Body);
            Console.WriteLine($"Procesando pedido {pedido.PedidoId} de cliente {pedido.ClienteId}");

            // Procesar aquí (ej: actualizar DB, llamar API logística, etc.)

            await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error procesando: {ex.Message}");
            // No borramos → mensaje se reintentará hasta DLQ
        }
    }
}
public record PedidoCreado(Guid PedidoId, Guid ClienteId, DateTimeOffset Fecha, decimal Total);
