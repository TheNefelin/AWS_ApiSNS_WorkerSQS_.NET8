using Amazon.SimpleNotificationService.Model;

namespace AWS_ClassLibrary.Services.Infrastructure;

public interface IAwsSnsService
{
    Task<string> SubscribeEmailAsync(string email, Dictionary<string, List<string>> filterPolicy);
    Task UnsubscribeAsync(string subscriptionArn);
    Task<string> PublishMessageAsync(string message, string subject, Dictionary<string, MessageAttributeValue>? messageAttributes = null);
    Task<Subscription?> GetExistingSubscriptionAsync(string email);
    Task<List<Subscription>> GetAllSubscriptionsAsync();
}
