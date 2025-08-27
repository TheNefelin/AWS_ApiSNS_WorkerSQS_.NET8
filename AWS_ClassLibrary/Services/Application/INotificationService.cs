using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;

namespace AWS_ClassLibrary.Services.Application;

public interface INotificationService
{
    Task<ApiResponse<string>> SubscribeEmailAsync(FormEmail formEmail);
    Task<ApiResponse<string>> UnsubscribeEmailAsync(FormEmail formEmail);
    Task<ApiResponse<string>> SendMassiveNotificationAsync(string message);
    Task SendDonationReceiptAsync(string email, int totalAmount, string downloadLink);
    Task SendDonationProcessingTaskAsync(FormDonation donation, CompanyDTO company, List<ProductDTO> products);
}
