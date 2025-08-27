
using Amazon.SQS.Model;

namespace AWS_ClassLibrary.Services.Infrastructure;

public interface IAwsSqsService
{
    Task<List<Message>> ReceiveMessagesAsync(int maxMessages = 10, int waitTimeSeconds = 20);
    Task DeleteMessageAsync(string receiptHandle);
    Task<int> GetApproximateMessageCountAsync();
}
