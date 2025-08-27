using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Services.Application;

namespace AWS_SQS_WebConsole_Worker.Services;

public class DonationProcessor : IDonationProcessor
{
    private readonly IDonationService _donationService;
    private readonly ILogger<DonationProcessor> _logger;

    public DonationProcessor(
        IDonationService donationService,
        ILogger<DonationProcessor> logger)
    {
        _donationService = donationService;
        _logger = logger;
    }

    public async Task ProcessDonationAsync(DonationTaskData donationData)
    {
        try
        {
            _logger.LogInformation("Iniciando procesamiento de donación para {Email} - ${TotalAmount}",
                donationData.Email, donationData.Products.Sum(p => p.Price));

            // Crear el task de procesamiento usando la estructura existente
            var processingTask = new DonationProcessingTask
            {
                TaskType = "ProcessDonation",
                Timestamp = DateTime.UtcNow,
                DonationData = donationData
            };

            // Usar el servicio existente para procesar la donación
            var result = await _donationService.ProcessDonationBackgroundAsync(processingTask);

            if (result.Status == "Completed")
            {
                _logger.LogInformation("Donación procesada exitosamente para {Email} - Estado: {Status}",
                    donationData.Email, result.Status);
            }
            else if (result.Status == "Failed")
            {
                _logger.LogError("Error procesando donación para {Email} - Error: {Error}",
                    donationData.Email, result.ErrorMessage);
                throw new InvalidOperationException($"Procesamiento falló: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando donación para {Email}", donationData.Email);
            throw;
        }
    }

    public async Task<bool> ProcessDonationWithRetriesAsync(DonationTaskData donationData, int maxRetries = 3)
    {
        int attempts = 0;

        while (attempts < maxRetries)
        {
            try
            {
                attempts++;
                await ProcessDonationAsync(donationData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Intento {Attempt}/{MaxRetries} fallido para donación de {Email}",
                    attempts, maxRetries, donationData.Email);

                if (attempts >= maxRetries)
                {
                    _logger.LogError(ex, "Falló el procesamiento tras {MaxRetries} intentos para {Email}",
                        maxRetries, donationData.Email);
                    return false;
                }

                // Espera exponencial entre reintentos
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempts));
                await Task.Delay(delay);
            }
        }

        return false;
    }
}
