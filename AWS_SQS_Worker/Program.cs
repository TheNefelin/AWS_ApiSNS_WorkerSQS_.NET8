// See https://aka.ms/new-console-template for more information
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL");

// Validación
if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(queueUrl))
{
    throw new InvalidOperationException("Faltan las variables de entorno AWS_REGION o QUEUE_URL.");
}
else
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
            // 1. Deserializar el sobre SNS
            var envelope = JsonSerializer.Deserialize<SnsEnvelope>(msg.Body);

            if (envelope?.Message == null)
            {
                Console.WriteLine("⚠️ Mensaje inválido o vacío en SNS.");
                continue;
            }

            // 2. Deserializar el JSON real dentro de "Message"
            var pedido = JsonSerializer.Deserialize<PedidoCreado>(envelope.Message);

            Console.WriteLine($"✅ Procesando pedido {pedido.PedidoId} de cliente {pedido.ClienteId}, total: {pedido.Total}");

            // Procesar aquí (ej: actualizar DB, llamar API logística, etc.)

            // 3. Borrar el mensaje de la cola después de procesarlo
            await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error procesando: {ex.Message}");
            // No borramos → mensaje se reintentará hasta DLQ
        }
    }
}

// Modelo del sobre SNS
public class SnsEnvelope
{
    public string Type { get; set; }
    public string MessageId { get; set; }
    public string TopicArn { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
}

// Modelo de tu evento Pedido
public record PedidoCreado(Guid PedidoId, Guid ClienteId, DateTimeOffset Fecha, decimal Total);
