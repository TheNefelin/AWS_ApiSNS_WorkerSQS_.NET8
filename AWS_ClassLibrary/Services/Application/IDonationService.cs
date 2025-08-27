using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;

namespace AWS_ClassLibrary.Services.Application;

public interface IDonationService
{
    Task<ApiResponse<string>> ProcessDonationRequestAsync(FormDonation donation);
    Task<DonationResultDTO> ProcessDonationBackgroundAsync(DonationProcessingTask task);
    Task<ApiResponse<string>> GetDonationReasonAsync();
}
