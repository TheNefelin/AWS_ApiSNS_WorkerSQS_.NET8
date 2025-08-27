using AWS_ClassLibrary.Models;

namespace AWS_SQS_WebConsole_Worker.Services;

public interface IDonationProcessor
{
    Task ProcessDonationAsync(DonationTaskData donationData);
    Task<bool> ProcessDonationWithRetriesAsync(DonationTaskData donationData, int maxRetries = 3);
}