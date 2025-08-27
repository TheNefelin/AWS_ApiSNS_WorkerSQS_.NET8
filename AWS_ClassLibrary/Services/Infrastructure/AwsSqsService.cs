using Amazon.SQS;
using Amazon.SQS.Model;
using AWS_ClassLibrary.Models;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services.Infrastructure;

public class AwsSqsService : IAwsSqsService
{
    private readonly ILogger<AwsSqsService> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly AWSOptions _awsOptions;

    public AwsSqsService(ILogger<AwsSqsService> logger, IAmazonSQS sqsClient, AWSOptions awsOptions)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _awsOptions = awsOptions;
    }

    public async Task<List<Message>> ReceiveMessagesAsync(int maxMessages = 10, int waitTimeSeconds = 20)
    {
        try
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = _awsOptions.SQS_URL,
                MaxNumberOfMessages = maxMessages,
                WaitTimeSeconds = waitTimeSeconds,
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(request);
            _logger.LogInformation("Mensajes recibidos de SQS: {MessageCount}", response.Messages.Count);

            return response.Messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recibiendo mensajes de SQS");
            throw;
        }
    }

    public async Task DeleteMessageAsync(string receiptHandle)
    {
        try
        {
            var deleteRequest = new DeleteMessageRequest
            {
                QueueUrl = _awsOptions.SQS_URL,
                ReceiptHandle = receiptHandle
            };

            await _sqsClient.DeleteMessageAsync(deleteRequest);
            _logger.LogDebug("Mensaje eliminado de SQS exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando mensaje de SQS");
            throw;
        }
    }

    public async Task<int> GetApproximateMessageCountAsync()
    {
        try
        {
            var request = new GetQueueAttributesRequest
            {
                QueueUrl = _awsOptions.SQS_URL,
                AttributeNames = new List<string> { "ApproximateNumberOfMessages" }
            };

            var response = await _sqsClient.GetQueueAttributesAsync(request);

            if (response.Attributes.TryGetValue("ApproximateNumberOfMessages", out string? countStr))
            {
                int count = int.Parse(countStr);
                _logger.LogDebug("Mensajes aproximados en cola: {MessageCount}", count);
                return count;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo número de mensajes en cola");
            return 0;
        }
    }
}