using Amazon;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using AWS_ClassLibrary.Context;
using AWS_ClassLibrary.Models;
using AWS_ClassLibrary.Repositories;
using AWS_ClassLibrary.Services.Application;
using AWS_ClassLibrary.Services.Infrastructure;
using AWS_ClassLibrary.Services.Utilities;
using AWS_SQS_WebConsole_Worker.Hubs;
using AWS_SQS_WebConsole_Worker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// VARIABLES DE ENTORNO
// ============================================================================
var AWS_REGION = builder.Configuration["AWS_REGION"] ?? "us-east-1";
var SNS_TOPIC_ARN = builder.Configuration["SNS_TOPIC_ARN"] ?? "arn:aws:sns:us-east-1:123:my-sns";
var S3_BUCKET_NAME = builder.Configuration["S3_BUCKET_NAME"] ?? "monolito-storage";
var SQS_URL = builder.Configuration["SQS_URL"] ?? "https://sqs.us-east-1.amazonaws.com/123/my-sqs";
var DB_HOST = builder.Configuration["DB_HOST"] ?? "localhost";
var DB_PORT = builder.Configuration["DB_PORT"] ?? "5432";
var DB_NAME = builder.Configuration["DB_NAME"] ?? "postgres";
var DB_USER = builder.Configuration["DB_USER"] ?? "postgres";
var DB_PASSWORD = builder.Configuration["DB_PASSWORD"] ?? "testing";

// ============================================================================
// CONFIGURACIÓN AWS OPTIONS
// ============================================================================
builder.Services.AddOptions<AWSOptions>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        settings.S3_BUCKET_NAME = S3_BUCKET_NAME;
        settings.SNS_TOPIC_ARN = SNS_TOPIC_ARN;
        settings.SQS_URL = SQS_URL;
    });

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<AWSOptions>>().Value);

// ============================================================================
// CONFIGURACIÓN BASE DE DATOS
// ============================================================================
var connectionString = $"Host={DB_HOST};Port={DB_PORT};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================================
// CONFIGURACIÓN AWS CLIENTES
// ============================================================================
builder.Services.AddSingleton<IAmazonSQS>(sp =>
    new AmazonSQSClient(RegionEndpoint.GetBySystemName(AWS_REGION)));

builder.Services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
    new AmazonSimpleNotificationServiceClient(RegionEndpoint.GetBySystemName(AWS_REGION)));

builder.Services.AddSingleton<IAmazonS3>(sp =>
    new AmazonS3Client(RegionEndpoint.GetBySystemName(AWS_REGION)));

// ============================================================================
// CONFIGURACIÓN DE REPOSITORIOS
// ============================================================================
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// ============================================================================
// CONFIGURACIÓN SERVICIOS DE INFRAESTRUCTURA
// ============================================================================
builder.Services.AddScoped<IAwsSqsService, AwsSqsService>();
builder.Services.AddScoped<IAwsSnsService, AwsSnsService>();
builder.Services.AddScoped<IAwsS3Service, AwsS3Service>();

// ============================================================================
// CONFIGURACIÓN SERVICIOS DE APLICACIÓN
// ============================================================================
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();

// ============================================================================
// CONFIGURACIÓN UTILIDADES
// ============================================================================
builder.Services.AddScoped<InvoiceGenerator>();

// ============================================================================
// CONFIGURACIÓN SERVICIO DE WORKER
// ============================================================================
builder.Services.AddScoped<DonationProcessor>();
builder.Services.AddSingleton<IConsoleSimulatorService, ConsoleSimulatorService>();

// ============================================================================
// CONFIGURACIÓN DE CONSOLA WEB
// ============================================================================
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ============================================================================
// CONFIGURACIÓN SERVICIO DE BACKGROUND 
// ============================================================================
builder.Services.AddHostedService<ConsoleSimulatorService>();

// 📝 Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapHub<ConsoleHub>("/consolehub");

app.Run();