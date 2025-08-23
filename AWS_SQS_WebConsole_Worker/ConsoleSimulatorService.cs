using Amazon.SQS;
using Amazon.SQS.Model;
using AWS_ClassLibrary;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace ConsoleSimulator;

public class ConsoleSimulatorService : BackgroundService
{
    private readonly IHubContext<ConsoleHub> _hubContext;
    private readonly IAmazonSQS _sqsClient;
    private readonly string _awsQueueUrl;

    public ConsoleSimulatorService(IHubContext<ConsoleHub> hubContext, IAmazonSQS sqsClient, AwsConfig awsConfig)
    {
        _hubContext = hubContext;
        _sqsClient = sqsClient;
        _awsQueueUrl = awsConfig.SQS_URL;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var resp = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _awsQueueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 20,
                    VisibilityTimeout = 30
                });

                foreach (var msg in resp.Messages)
                {
                    var envelope = JsonSerializer.Deserialize<SnsEnvelope>(msg.Body);

                    var message = $"[{DateTime.Now:T}] Mensaje recibido: [MessageId = {msg.MessageId}]";
                    Console.WriteLine(message);
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

                    if (envelope?.Message == null)
                    {
                        message = $"⚠️ [{DateTime.Now:T}] Mensaje inválido o vacío en SNS.";
                        Console.WriteLine(message);
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
                        continue;
                    }

                    var pedido = JsonSerializer.Deserialize<Order>(envelope.Message);
                    if (pedido == null)
                    {
                        message = $"⚠️ [{DateTime.Now:T}] No se pudo deserializar el pedido";
                        Console.WriteLine(message);
                        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
                        continue;
                    }

                    message = $"✅ [{DateTime.Now:T}] Procesando Pedido: {pedido.PedidoId}, Cliente: {pedido.ClienteId}, Total: {pedido.Total}";
                    Console.WriteLine(message);
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

                    //  (guardar en DB, llamar servicio, etc.)

                    await _sqsClient.DeleteMessageAsync(_awsQueueUrl, msg.ReceiptHandle);
                }
            } catch (Exception ex)
            {
                var message = $"❌ [{DateTime.Now:T}] Error - {ex.Message}";
                Console.WriteLine(message);
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}