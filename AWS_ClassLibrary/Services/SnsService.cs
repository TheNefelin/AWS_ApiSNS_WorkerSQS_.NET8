using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using Newtonsoft.Json;

namespace AWS_ClassLibrary.Services;

public class SnsService
{
    private readonly SnsConfig _snsConfig;
    private readonly IAmazonSimpleNotificationService _snsClient;

    public SnsService(SnsConfig snsConfig, IAmazonSimpleNotificationService snsClient)
    {
        _snsConfig = snsConfig;
        _snsClient = snsClient;
    }

    // Método de Subscribe con la política de filtro
    public async Task<ApiResponse<string>> Subscribe(FormSubscribe formSubscribe)
    {
        try
        {
            var existingSubscription = await ExistingSubscription(formSubscribe.Email);
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
                { "user_email", new List<string> { formSubscribe.Email } },
                { "tipo_notificacion", new List<string> { "masiva" } }
            };

            var request = new SubscribeRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Protocol = "email",
                Endpoint = formSubscribe.Email,
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
            Console.WriteLine($"Error subscribing {formSubscribe.Email} to {_snsConfig.SNS_TOPIC_ARN}: {ex.Message}");
            
            return new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Message = $"Error interno del servidor al suscribirse: {ex}",
            };
        }
    }

    // Método para enviar un mensaje individual (la factura de la donación)
    public async Task<string> PublishIndividualDonation(FormDonation formDonation)
    {
        try
        {
            if (formDonation.Amount < 1 || formDonation.Amount > 3)
            {
                return "La cantidad de donacion debe ser entre 1 y 3";
            }

            var existingSubscription = await ExistingSubscription(formDonation.Email);
            if (existingSubscription == null)
            {
                return "No estás suscrito.";
            }
            else
            {
                if (existingSubscription.SubscriptionArn == "PendingConfirmation")
                {
                    return "Solicitud de suscripción ya enviada. Revisa tu correo para confirmar.";
                }
            }

            var json = JsonConvert.SerializeObject(formDonation);

            var request = new PublishRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Message = json,
                Subject = "¡Gracias por tu donación!",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    // Este atributo de mensaje coincide con la política de filtro de un solo usuario
                    {
                        "user_email",
                        new MessageAttributeValue { DataType = "String", StringValue = formDonation.Email }
                    }
                }
            };

            var response = await _snsClient.PublishAsync(request);

            return $"Gracias por tu donación [{response.MessageId}], recibirás la factura con los regalos donados.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending donation notification: {ex.Message}");
            throw;
        }
    }

    // Método para enviar un mensaje masivo (a todos los suscriptores)
    public async Task<string> PublishMassiveNotification(string messageContent)
    {
        var request = new PublishRequest
        {
            TopicArn = _snsConfig.SNS_TOPIC_ARN,
            Message = messageContent,
            Subject = "Actualización Importante sobre Donaciones",
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                // Este atributo de mensaje coincide con la política de filtro para mensajes masivos
                {
                    "tipo_notificacion",
                    new MessageAttributeValue { DataType = "String", StringValue = "masiva" }
                }
            }
        };

        var response = await _snsClient.PublishAsync(request);
        return response.MessageId;
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
