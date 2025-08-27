using Amazon.SQS.Model;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services.Infrastructure;
using AWS_SQS_WebConsole_Worker.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AWS_SQS_WebConsole_Worker.Services;

public class ConsoleSimulatorService : BackgroundService
{
    private readonly ILogger<ConsoleSimulatorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<ConsoleHub> _hubContext;

    private int _processedCount = 0;
    private int _errorCount = 0;
    private readonly object _lockObject = new object();

    public ConsoleSimulatorService(
        ILogger<ConsoleSimulatorService> logger,
        IServiceProvider serviceProvider,
        IHubContext<ConsoleHub> hubContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SendWelcomeMessage();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var sqsService = scope.ServiceProvider.GetRequiredService<IAwsSqsService>();
                var donationProcessor = scope.ServiceProvider.GetRequiredService<DonationProcessor>();

                var messages = await sqsService.ReceiveMessagesAsync(10, 20);

                if (!messages.Any())
                {
                    SendConsoleMessage(".", "waiting", false);
                    await Task.Delay(2000, stoppingToken);
                    continue;
                }

                SendConsoleMessage($"[{DateTime.Now:HH:mm:ss}] Recibidos {messages.Count} mensajes de SQS", "info");

                var tasks = messages.Select(async message =>
                {
                    try
                    {
                        await ProcessMessage(message, sqsService, donationProcessor);
                        await sqsService.DeleteMessageAsync(message.ReceiptHandle);

                        lock (_lockObject)
                        {
                            _processedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        SendConsoleMessage($"Error procesando mensaje {message.MessageId}: {ex.Message}", "error");
                        lock (_lockObject)
                        {
                            _errorCount++;
                        }
                    }
                });

                await Task.WhenAll(tasks);
                SendStatisticsMessage();
            }
            catch (Exception ex)
            {
                SendConsoleMessage($"Error en el bucle principal del worker: {ex.Message}", "error");
                await Task.Delay(5000, stoppingToken);
            }
        }

        SendConsoleMessage("Worker detenido por cancelacion", "warning");
    }

    private async Task ProcessMessage(Message message, IAwsSqsService sqsService, DonationProcessor donationProcessor)
    {
        try
        {
            var snsMessage = JsonSerializer.Deserialize<SnsMessage>(message.Body);

            if (snsMessage == null || string.IsNullOrEmpty(snsMessage.Message))
            {
                SendConsoleMessage($"Mensaje SNS invalido: {message.MessageId}", "warning");
                return;
            }

            var processingTask = JsonSerializer.Deserialize<DonationProcessingTask>(snsMessage.Message);

            if (processingTask?.DonationData == null)
            {
                SendConsoleMessage($"Datos de donacion invalidos en mensaje: {message.MessageId}", "warning");
                return;
            }

            var donationData = processingTask.DonationData;
            var totalAmount = donationData.Products.Sum(p => p.Price);

            SendConsoleMessage($"Procesando donacion:", "processing");
            SendConsoleMessage($"   ID: {message.MessageId[..8]}...", "processing");
            SendConsoleMessage($"   Cantidad: {donationData.Amount} productos", "processing");
            SendConsoleMessage($"   Total: ${totalAmount:F2}", "processing");
            SendConsoleMessage($"   Email: {donationData.Email}", "processing");
            SendConsoleMessage($"   Empresa: {donationData.Company.Name}", "processing");

            await donationProcessor.ProcessDonationAsync(donationData);

            SendConsoleMessage($"Donacion procesada exitosamente: ${totalAmount:F2}", "success");
        }
        catch (JsonException jsonEx)
        {
            SendConsoleMessage($"Error de serializacion en mensaje {message.MessageId}: {jsonEx.Message}", "error");
            _logger.LogError(jsonEx, "Error de JSON en mensaje {MessageId}", message.MessageId);
        }
        catch (Exception ex)
        {
            SendConsoleMessage($"Error procesando donacion en mensaje {message.MessageId}: {ex.Message}", "error");
            _logger.LogError(ex, "Error procesando mensaje {MessageId}", message.MessageId);
            throw;
        }
    }

    private void SendWelcomeMessage()
    {
        SendConsoleMessage("╔═══════════════════════════════════════════════════════════╗", "info");
        SendConsoleMessage("║                   DONATION PROCESSOR                      ║", "info");
        SendConsoleMessage("║                   AWS SQS Worker Console                  ║", "info");
        SendConsoleMessage("╚═══════════════════════════════════════════════════════════╝", "info");
        SendConsoleMessage($"Iniciado a las: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", "info");
        SendConsoleMessage("Worker ejecutandose en segundo plano", "info");
        SendConsoleMessage("Estado: Esperando mensajes...", "info");
    }

    private void SendStatisticsMessage()
    {
        var stats = $"Procesados: {_processedCount} | Errores: {_errorCount} | Total: {_processedCount + _errorCount}";
        SendConsoleMessage(stats, "stats");
    }

    private void SendConsoleMessage(string message, string type, bool addTimestamp = true)
    {
        var timestamp = addTimestamp ? $"[{DateTime.Now:HH:mm:ss.fff}] " : "";
        var formattedMessage = $"{timestamp}{message}";

        var consoleData = new
        {
            message = formattedMessage,
            type = type,
            timestamp = DateTime.Now
        };

        var jsonMessage = JsonSerializer.Serialize(consoleData);

        _hubContext.Clients.All.SendAsync("ConsoleUpdate", jsonMessage);
        _logger.LogInformation("[CONSOLE] {Message}", formattedMessage);
    }
}