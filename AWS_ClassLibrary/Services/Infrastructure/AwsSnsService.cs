using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AWS_ClassLibrary.Services.Infrastructure;

public class AwsSnsService : IAwsSnsService
{
    private readonly ILogger<AwsSnsService> _logger;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly AWSOptions _awsOptions;

    public AwsSnsService(ILogger<AwsSnsService> logger, IAmazonSimpleNotificationService snsClient, AWSOptions awsOptions)
    {
        _logger = logger;
        _snsClient = snsClient;
        _awsOptions = awsOptions;
    }

    public async Task<string> SubscribeEmailAsync(string email, Dictionary<string, List<string>> filterPolicy)
    {
        try
        {
            var request = new SubscribeRequest
            {
                TopicArn = _awsOptions.SNS_TOPIC_ARN,
                Protocol = "email",
                Endpoint = email,
                Attributes = new Dictionary<string, string>
                {
                    { "FilterPolicy", JsonConvert.SerializeObject(filterPolicy) }
                }
            };

            var response = await _snsClient.SubscribeAsync(request);
            _logger.LogInformation("Email suscrito a SNS: {Email}, ARN: {SubscriptionArn}", email, response.SubscriptionArn);

            return response.SubscriptionArn;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suscribiendo email a SNS: {Email}", email);
            throw;
        }
    }

    public async Task UnsubscribeAsync(string subscriptionArn)
    {
        try
        {
            await _snsClient.UnsubscribeAsync(subscriptionArn);
            _logger.LogInformation("Suscripción cancelada: {SubscriptionArn}", subscriptionArn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelando suscripción: {SubscriptionArn}", subscriptionArn);
            throw;
        }
    }

    public async Task<string> PublishMessageAsync(string message, string subject, Dictionary<string, MessageAttributeValue>? messageAttributes = null)
    {
        try
        {
            var request = new PublishRequest
            {
                TopicArn = _awsOptions.SNS_TOPIC_ARN,
                Message = message,
                Subject = subject,
                MessageAttributes = messageAttributes ?? new Dictionary<string, MessageAttributeValue>()
            };

            var response = await _snsClient.PublishAsync(request);
            _logger.LogInformation("Mensaje publicado en SNS: {MessageId}", response.MessageId);

            return response.MessageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando mensaje en SNS");
            throw;
        }
    }

    public async Task<Subscription?> GetExistingSubscriptionAsync(string email)
    {
        try
        {
            var subscriptionList = await _snsClient.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
            {
                TopicArn = _awsOptions.SNS_TOPIC_ARN
            });

            var subscription = subscriptionList?.Subscriptions?.FirstOrDefault(s => s.Endpoint == email);
            _logger.LogDebug("Búsqueda de suscripción para {Email}: {Found}", email, subscription != null ? "Encontrada" : "No encontrada");

            return subscription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando suscripción para email: {Email}", email);
            throw;
        }
    }

    public async Task<List<Subscription>> GetAllSubscriptionsAsync()
    {
        try
        {
            var subscriptionList = await _snsClient.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
            {
                TopicArn = _awsOptions.SNS_TOPIC_ARN
            });

            var subscriptions = subscriptionList?.Subscriptions ?? new List<Subscription>();
            _logger.LogDebug("Total de suscripciones obtenidas: {Count}", subscriptions.Count);

            return subscriptions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todas las suscripciones");
            throw;
        }
    }
}