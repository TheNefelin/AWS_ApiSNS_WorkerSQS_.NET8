namespace AWS_ClassLibrary.Models;

public class DonationProcessingTask
{
    public string TaskType { get; set; } = "ProcessDonation";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DonationTaskData DonationData { get; set; }
}
