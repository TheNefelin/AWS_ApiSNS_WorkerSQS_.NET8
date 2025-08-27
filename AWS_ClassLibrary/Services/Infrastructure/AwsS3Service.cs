using Amazon.S3;
using Amazon.S3.Model;
using AWS_ClassLibrary.Models;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services.Infrastructure;

public class AwsS3Service : IAwsS3Service
{
    private readonly ILogger<AwsS3Service> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly AWSOptions _awsOptions;

    public AwsS3Service(ILogger<AwsS3Service> logger, IAmazonS3 s3Client, AWSOptions awsOptions)
    {
        _logger = logger;
        _s3Client = s3Client;
        _awsOptions = awsOptions;
    }

    public async Task<string> SavePdfToBucketAsync(Stream pdfStream)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"factura_{timestamp}.pdf";

            var request = new PutObjectRequest
            {
                BucketName = _awsOptions.S3_BUCKET_NAME,
                Key = $"docs/{fileName}",
                InputStream = pdfStream,
                ContentType = "application/pdf"
            };

            await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("Archivo PDF subido a S3: {FileName}", fileName);

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo PDF a S3");
            throw;
        }
    }

    public async Task<Stream> GetFileStreamFromBucketAsync(string fileName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _awsOptions.S3_BUCKET_NAME,
                Key = $"images/{fileName}"
            };

            var response = await _s3Client.GetObjectAsync(request);
            _logger.LogDebug("Archivo obtenido de S3: {FileName}", fileName);

            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener archivo de S3: {FileName}", fileName);
            throw;
        }
    }

    public string GeneratePreSignedUrl(string fileName, TimeSpan duration)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _awsOptions.S3_BUCKET_NAME,
                Key = $"docs/{fileName}",
                Expires = DateTime.UtcNow.Add(duration)
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogDebug("URL pre-firmada generada para: {FileName}", fileName);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando URL pre-firmada para: {FileName}", fileName);
            throw;
        }
    }
}