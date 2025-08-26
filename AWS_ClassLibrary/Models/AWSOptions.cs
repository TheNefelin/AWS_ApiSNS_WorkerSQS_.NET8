namespace AWS_ClassLibrary.Models;

public class AWSOptions
{
    public required string S3_BUCKET_NAME { get; set; }
    public required string SNS_TOPIC_ARN { get; set; }
    public required string SQS_URL { get; set; }
}
