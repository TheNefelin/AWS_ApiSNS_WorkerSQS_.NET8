using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
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

    public async Task<string> Subscribe(string email)
    {
        try
        {
            var message = await validateSubscription(email);

            if (message != null)
            {
                return message;
            }

            // Política de filtro
            var filterPolicy = new Dictionary<string, List<string>>
            {
                { "user_email", new List<string> { email } },
                { "tipo_notificacion", new List<string> { "masiva" } }
            };

            var request = new SubscribeRequest
            {
                TopicArn = _snsConfig.SNS_TOPIC_ARN,
                Protocol = "email",
                Endpoint = email,
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
            Console.WriteLine($"Error subscribing {email} to {_snsConfig.SNS_TOPIC_ARN}: {ex.Message}");
            throw;
        }
    }

    public async Task<string> Donate(Donation donation)
    {
        try
        {

        } catch (Exception ex)
        {
            Console.WriteLine($"Error sending donation notification: {ex.Message}");
            throw;
        }
        return "";
    }


    private async Task<string> validateSubscription(string email)
    {
        var subscriptionList = await _snsClient.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
        {
            TopicArn = _snsConfig.SNS_TOPIC_ARN
        });

        var existingSubscription = subscriptionList.Subscriptions.FirstOrDefault(s => s.Endpoint == email);

        if (existingSubscription == null)
            return null;

        if (existingSubscription.SubscriptionArn == "PendingConfirmation")
            return "Solicitud de suscripción ya enviada. Revisa tu correo para confirmar.";

        return "Ya estás suscrito.";
    }
}
