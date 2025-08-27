namespace AWS_SQS_WebConsole_Worker.Services;

public interface IConsoleSimulatorService
{
    Task StartAsync(CancellationToken cancellationToken);
    event Action<string> OnConsoleUpdate;
}
