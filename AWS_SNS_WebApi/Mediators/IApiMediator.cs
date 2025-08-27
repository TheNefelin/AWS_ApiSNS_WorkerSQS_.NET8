using AWS_ClassLibrary.DTOs;
using AWS_ClassLibrary.Models;

namespace AWS_SNS_WebApi.Mediators;

public interface IApiMediator
{
    Task<ApiResponse<string>> GetDonationReasonAsync();
    Task<ApiResponse<IEnumerable<CompanyDTO>>> GetCompaniesAsync();
    Task<ApiResponse<IEnumerable<ProductDTO>>> GetProductsAsync();
    Task<ApiResponse<string>> SubscribeEmailAsync(FormEmail formEmail);
    Task<ApiResponse<string>> UnsubscribeEmailAsync(FormEmail formEmail);
    Task<ApiResponse<string>> ProcessDonationAsync(FormDonation formDonation);
    Task<ApiResponse<string>> SendMassiveNotificationAsync(string message);
}
