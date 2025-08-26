using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AWS_ClassLibrary.Models;
using Microsoft.Extensions.Logging;

namespace AWS_ClassLibrary.Services;

public class S3Service
{
    private readonly ILogger<S3Service> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly AWSOptions _awsOptions;

    public S3Service(ILogger<S3Service> logger, IAmazonS3 s3Client, AWSOptions awsOptions)
    {
        _logger = logger;
        _s3Client = s3Client;
        _awsOptions = awsOptions;
    }

    public async Task<string> SavePdfToBucket(Stream pdfStream)
    {
        try
        {
            // Obtenemos un nombre de archivo único con fecha y hora.
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"factura_{timestamp}.pdf";

            var request = new PutObjectRequest
            {
                BucketName = _awsOptions.S3_BUCKET_NAME,
                Key = $"docs/{fileName}",
                InputStream = pdfStream,
                ContentType = "application/pdf"
            };

            // Subimos el stream del PDF a S3.
            await _s3Client.PutObjectAsync(request);

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir el archivo a S3.");
            throw;
        }
    }

    public string GeneratePreSignedUrl(string fileName, TimeSpan duration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _awsOptions.S3_BUCKET_NAME,
            Key = $"docs/{fileName}",
            Expires = DateTime.UtcNow.Add(duration)
        };
        return _s3Client.GetPreSignedURL(request);
    }

    // Este método descarga un archivo de S3 como un Stream
    public async Task<Stream> GetFileStreamFromBucket(string fileName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _awsOptions.S3_BUCKET_NAME,
                Key =  $"images/{fileName}"
            };

            var response = await _s3Client.GetObjectAsync(request);
            // Devuelve el stream del objeto
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el archivo de S3.");
            throw;
        }
    }
}
