using Microsoft.AspNetCore.SignalR;

namespace AWS_SQS_WebConsole_Worker.Hubs;

public class ConsoleHub : Hub
{
    public async Task JoinConsole()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Console");
    }

    public async Task LeaveConsole()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Console");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Console");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Console");
        await base.OnDisconnectedAsync(exception);
    }
}
