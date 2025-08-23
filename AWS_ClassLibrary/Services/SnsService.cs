using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AWS_ClassLibrary.Services;

public class SnsService
{
    private readonly ILogger<SnsService> _logger;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly SnsConfig _snsConfig;

    public SnsService(ILogger<SnsService> logger, IAmazonSimpleNotificationService snsClient, SnsConfig snsConfig)
    {
        _logger = logger;
        _snsClient = snsClient;
        _snsConfig = snsConfig;
    }

    // Método de Subscribe con la política de filtro
    public async Task<ApiResponse<string>> Subscribe(FormEmail formEmail)
    {
        try
        {
            var existingSubscription = await ExistingSubscription(formEmail.Email);
            if (existingSubscription != null)
            {
                if (existingSubscription.SubscriptionArn == "PendingConfirmation")
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Solicitud de suscripción ya enviada. Revisa tu correo para confirmar.",
                    };
                }
                else
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Ya estás suscrito.",
                    };
                }
            }

            // Política de filtro: el usuario recibirá mensajes si su user_email coincide
            // O si el mensaje tiene el atributo tipo_notificacion='masiva'.
            var filterPolicy = new Dictionary<string, List<string>>
            {
                { "user_email", new List<string> { formEmail.Email } },
                { "massive_notification", new List<string> { "true" } }
            };

            var request = new SubscribeRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Protocol = "email",
                Endpoint = formEmail.Email,
                Attributes = new Dictionary<string, string>
                {
                    { "FilterPolicy", JsonConvert.SerializeObject(filterPolicy) }
                }
            };

            var response = await _snsClient.SubscribeAsync(request);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = "Solicitud de suscripción enviada. Revisa tu correo para confirmar.",
                Data = response.SubscriptionArn
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al suscribirse");

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = $"Error interno del servidor al suscribirse: {ex}",
            };
        }
    }

    public async Task<ApiResponse<string>> Unsubscribe(FormEmail formEmail)
    {
        try
        {
            var subscription = await ExistingSubscription(formEmail.Email);

            if (subscription == null || subscription.SubscriptionArn == "PendingConfirmation")
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 404,
                    Message = "No se encontró una suscripción activa para este correo."
                };
            }

            await _snsClient.UnsubscribeAsync(subscription.SubscriptionArn);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = $"El correo {formEmail.Email} ha sido desuscrito correctamente."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desuscribirse");

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = $"Error interno al desuscribirse: {ex}"
            };
        }
    }


    // Método para enviar un mensaje individual (la factura de la donación)
    public async Task<ApiResponse<string>> PublishIndividualDonation(FormDonation formDonation)
    {
        try
        {
            if (formDonation.Amount < 1 || formDonation.Amount > 3)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "La cantidad de donacion debe ser entre 1 y 3",
                };
            }

            var existingSubscription = await ExistingSubscription(formDonation.Email);
            if (existingSubscription == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "No estás suscrito.",
                };
            }
            else
            {
                if (existingSubscription.SubscriptionArn == "PendingConfirmation")
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Debes aceptar la suscripción. Revisa tu correo para confirmar.",
                    };
                }
            }

            //var json = JsonConvert.SerializeObject(formDonation);

            string htmlMessage = $@"
                <html>
                  <body>
                    <h2>Gracias por tu pedido #{123456}</h2>
                    <p>Hola {formDonation.Email},</p>
                    <p>Tu pedido fue registrado con éxito.</p>
                    <p>Total: {formDonation.Amount:C}</p>
                  </body>
                </html>";

            var request = new PublishRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Subject = "¡Gracias por tu donación!",
                //Message = json,
                Message = htmlMessage,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    // Este atributo de mensaje coincide con la política de filtro de un solo usuario
                    {
                        "user_email",
                        new MessageAttributeValue 
                        { 
                            DataType = "String", 
                            StringValue = formDonation.Email 
                        }
                    }
                }
            };

            var response = await _snsClient.PublishAsync(request);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = $"Gracias por tu donación [{response.MessageId}], recibirás la factura con los regalos donados.",
                Data = formDonation.Amount.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar la notificación de donación");

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = $"Error interno del servidor al enviar la notificación de donación: {ex}",
            };
        }
    }

    // Método para enviar un mensaje masivo (a todos los suscriptores)
    public async Task<ApiResponse<string>> PublishMassiveNotification(string messageContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "El contenido del mensaje no puede estar vacío.",
                };
            }

            string htmlMessage = $@"
                <html>
                  <body>
                    <h2>Actualización Importante sobre Donaciones</h2>
                    <p>{messageContent}</p>
                    <p><i>Este correo fue generado automáticamente por nuestro sistema de donaciones.</i></p>
                  </body>
                </html>";

            var request = new PublishRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Subject = "Actualización Importante sobre Donaciones",
                Message = htmlMessage,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    // Este atributo de mensaje coincide con la política de filtro para mensajes masivos
                    {
                        "massive_notification",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = "true"
                        }
                    }
                }
            };

            var response = await _snsClient.PublishAsync(request);

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = 200,
                Message = $"Notificación masiva enviada con éxito [{response.MessageId}].",
                Data = messageContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar la notificación masiva");

            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = $"Error interno del servidor al enviar la notificación masiva: {ex}",
            };
        }
    }

    private async Task<Subscription?> ExistingSubscription(string email)
    {
        var subscriptionList = await _snsClient.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
        {
            TopicArn = _snsConfig.SNS_TOPIC_ARN
        });

        if (subscriptionList?.Subscriptions == null)
        {
            return null;
        }

        return subscriptionList.Subscriptions.FirstOrDefault(s => s.Endpoint == email);
    }
}
