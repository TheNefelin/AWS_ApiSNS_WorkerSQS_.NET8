using Amazon;
using Amazon.SQS;
using AWS_ClassLibrary;
using ConsoleSimulator;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

var AWS_REGION = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
var AWS_SQS_QUEUE_URL = Environment.GetEnvironmentVariable("AWS_SQS_QUEUE_URL") ?? "https://sqs.us-east-1.amazonaws.com/123/ecomexpress-sqs-worker";

builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    return new AmazonSQSClient(RegionEndpoint.GetBySystemName(AWS_REGION));
});
builder.Services.AddSingleton(new AwsConfig
{
    SQS_URL = AWS_SQS_QUEUE_URL,
    SNS_TOPIC_ARN = ""
});
builder.Services.AddSignalR();
builder.Services.AddHostedService<ConsoleSimulatorService>();

var app = builder.Build();

// ✅ ESTAS DOS LÍNEAS SON CRÍTICAS:
app.UseDefaultFiles();    // Busca index.html por defecto
app.UseStaticFiles();     // Sirve archivos estáticos

// Hub de SignalR
app.MapHub<ConsoleHub>("/consoleHub");

app.Run();

// Hub para comunicación en tiempo real
public class ConsoleHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}