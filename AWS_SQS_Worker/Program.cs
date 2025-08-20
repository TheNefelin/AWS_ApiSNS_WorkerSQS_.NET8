// See https://aka.ms/new-console-template for more information
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

// 1. Configure Environment Variables
var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL");

// 2. Validate the environment variables
if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(queueUrl))
{
    throw new InvalidOperationException("Faltan las variables de entorno AWS_REGION o SQS_QUEUE_URL.");
}

// 3. Create SQS Client
var sqs = new AmazonSQSClient(RegionEndpoint.GetBySystemName(region));

// 4. Start the message processing loop
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
            // 1️⃣ Deserializar el sobre SNS
            var envelope = JsonSerializer.Deserialize<SnsEnvelope>(msg.Body);

            if (envelope?.Message == null)
            {
                Console.WriteLine("⚠️ Mensaje inválido o vacío en SNS.");
                continue;
            }

            // 2️⃣ Deserializar el JSON real dentro de "Message"
            var pedido = JsonSerializer.Deserialize<PedidoCreado>(envelope.Message);

            Console.WriteLine($"✅ Procesando pedido {pedido.PedidoId} de cliente {pedido.ClienteId}, total: {pedido.Total}");

            // 3️⃣ Procesar (guardar en DB, llamar servicio, etc.)

            // 4️⃣ Borrar el mensaje
            await sqs.DeleteMessageAsync(queueUrl, msg.ReceiptHandle);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error procesando: {ex.Message}");
            // No borrar → SQS reintentará o DLQ
        }
    }
}


// 5. SNS message envelope model
public class SnsEnvelope
{
    public string Type { get; set; }
    public string MessageId { get; set; }
    public string TopicArn { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
}

// 6. PedidoCreado event model
public record PedidoCreado(Guid PedidoId, Guid ClienteId, DateTimeOffset Fecha, decimal Total);
