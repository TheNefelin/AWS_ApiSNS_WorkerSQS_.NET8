namespace AWS_ClassLibrary.Models;

public class SnsMessage
{
    public string Type { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public string TopicArn { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public Dictionary<string, SnsMessageAttribute> MessageAttributes { get; set; } = new();
}
