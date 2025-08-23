using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS_ClassLibrary.Models;
using Newtonsoft.Json;
using System.Net;

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

    public async Task<string> Subscribe(FormSubscribe formSubscribe)
    {
        try
        {
            var existingSubscription = await ExistingSubscription(formSubscribe.Email);
            if (existingSubscription != null) {
                if (existingSubscription.SubscriptionArn == "PendingConfirmation")
                {
                    return "Solicitud de suscripción ya enviada. Revisa tu correo para confirmar.";
                }
                else
                {
                    return "Ya estás suscrito.";
                }
            }

            // Política de filtro
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

            return response.SubscriptionArn;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error subscribing {formSubscribe.Email} to {_snsConfig.SNS_TOPIC_ARN}: {ex.Message}");
            throw;
        }
    }

    public async Task<string> Donate(Donation donation)
    {
        try
        {
            if (donation.Amount < 1 || donation.Amount > 3)
            {
                return "La cantidad de donacion debe ser entre 1 y 3";
            }

            var existingSubscription = await ExistingSubscription(donation.Email);
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

            var json = System.Text.Json.JsonSerializer.Serialize(donation);

            await _snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Message = json,
                Subject = "Nueva Donación"
            });

            return "Gracias por tu donación, recibirás la factura con los regalos donados.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending donation notification: {ex.Message}");
            throw;
        }
    }

    private async Task<Subscription?> ExistingSubscription(string email)
    {
        var subscriptionList = await _snsClient.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
        {
            TopicArn = _snsConfig.SNS_TOPIC_ARN
        });

        return subscriptionList.Subscriptions.FirstOrDefault(s => s.Endpoint == email);
    }
}
