using Amazon.SimpleNotificationService.Model;
using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AWS_ClassLibrary.Services.Application;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IAwsSnsService _snsService;

    public NotificationService(ILogger<NotificationService> logger, IAwsSnsService snsService)
    {
        _logger = logger;
        _snsService = snsService;
    }

    public async Task<ApiResponse<string>> SubscribeEmailAsync(FormEmail formEmail)
    {
        try
        {
            // Verificar si ya existe suscripción
            var existingSubscription = await _snsService.GetExistingSubscriptionAsync(formEmail.Email);

            if (existingSubscription != null)
            {
                if (existingSubscription.SubscriptionArn == "PendingConfirmation")
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Solicitud de suscripción ya enviada. Revisa tu correo para confirmar."
                    };
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Ya estás suscrito a las notificaciones."
                };
            }

            // Crear política de filtro para el usuario
            var filterPolicy = new Dictionary<string, List<string>>
            {
                { "target", new List<string> { formEmail.Email, "all" } }
            };

            var subscriptionArn = await _snsService.SubscribeEmailAsync(formEmail.Email, filterPolicy);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = "Solicitud de suscripción enviada. Revisa tu correo para confirmar.",
                Data = subscriptionArn
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio de suscripción para: {Email}", formEmail.Email);

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno del servidor al procesar la suscripción."
            };
        }
    }

    public async Task<ApiResponse<string>> UnsubscribeEmailAsync(FormEmail formEmail)
    {
        try
        {
            var subscription = await _snsService.GetExistingSubscriptionAsync(formEmail.Email);

            if (subscription == null || subscription.SubscriptionArn == "PendingConfirmation")
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "No se encontró una suscripción activa para este correo."
                };
            }

            await _snsService.UnsubscribeAsync(subscription.SubscriptionArn);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = $"El correo {formEmail.Email} ha sido desuscrito correctamente."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en servicio de desuscripción para: {Email}", formEmail.Email);

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno del servidor al procesar la desuscripción."
            };
        }
    }

    public async Task<ApiResponse<string>> SendMassiveNotificationAsync(string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "El contenido del mensaje no puede estar vacío."
                };
            }

            var messageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "target",
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = "all"
                    }
                }
            };

            var messageId = await _snsService.PublishMessageAsync(
                message,
                "Actualización Importante sobre Donaciones",
                messageAttributes);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = $"Notificación masiva enviada con éxito [{messageId}].",
                Data = message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación masiva");

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = "Error interno del servidor al enviar la notificación masiva."
            };
        }
    }

    public async Task SendDonationReceiptAsync(string email, int totalAmount, string downloadLink)
    {
        try
        {
            var existingSubscription = await _snsService.GetExistingSubscriptionAsync(email);

            if (existingSubscription == null)
            {
                _logger.LogWarning("Usuario {Email} no está suscrito - enviando PDF por correo individual", email);
                return;
            }

            if (existingSubscription.SubscriptionArn == "PendingConfirmation")
            {
                _logger.LogWarning("Usuario {Email} debe confirmar suscripción - enviando PDF por correo individual", email);
                return;
            }

            string textMessage = $@"
                🎉 ¡GRACIAS POR TU DONACIÓN! 🎉

                Hola {email},

                ✅ Tu donación fue registrada con éxito.

                📝 DETALLES DE TU PEDIDO:
                   • Pedido: #{Random.Shared.Next(10000000, 99999999)}
                   • Total: ${totalAmount:N0}
                   • Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}

                🎁 ¡Muchas gracias por tu generosidad!

                🔗 DESCARGA TU FACTURA AQUÍ: {downloadLink}

                ---
                Este correo fue generado automáticamente por nuestro sistema de donaciones.
                ---";

            var messageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "target",
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = email
                    }
                }
            };

            await _snsService.PublishMessageAsync(
                textMessage.Trim(),
                "🎉 ¡Gracias por tu donación!",
                messageAttributes);

            _logger.LogInformation("Recibo de donación enviado a: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando recibo de donación a: {Email}", email);
            throw;
        }
    }

    public async Task SendDonationProcessingTaskAsync(FormDonation donation, CompanyDTO company, List<ProductDTO> products)
    {
        try
        {
            var processingTask = new DonationProcessingTask
            {
                TaskType = "ProcessDonation",
                Timestamp = DateTime.UtcNow,
                DonationData = new DonationTaskData
                {
                    Email = donation.Email,
                    Amount = donation.Amount,
                    Company = company,
                    Products = products
                }
            };

            var messageBody = JsonConvert.SerializeObject(processingTask);

            var messageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "messageType",
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = "ProcessingTask"
                    }
                },
                {
                    "target",
                    new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = "worker"
                    }
                }
            };

            var messageId = await _snsService.PublishMessageAsync(
                messageBody,
                "Donation Processing Task",
                messageAttributes);

            _logger.LogInformation("Tarea de donación enviada al Worker: {MessageId} para {Email}", messageId, donation.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando tarea al Worker para: {Email} - pero no afecta la donación", donation.Email);
            // NO lanzar excepción para no afectar el flujo principal
        }
    }
}
