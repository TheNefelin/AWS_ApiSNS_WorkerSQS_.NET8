namespace AWS_SQS_Worker;

// Modelo del sobre SNS
public class SnsEnvelope
{
    public string Type { get; set; }
    public string MessageId { get; set; }
    public string TopicArn { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
}