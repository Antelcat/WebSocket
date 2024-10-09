using Antelcat.AspNetCore.WebSocket;
using Antelcat.AspNetCore.WebSocket.Extensions;

namespace Antelcat.WebSocket.TestWeb;

public class SampleWebSocketClient : Client
{
    protected override async Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
    }

    protected override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"{Context.ConnectionId} disconnected due to {exception}");
    }

    protected override async Task OnReceivedTextAsync(string text, CancellationToken token)
    {
        await this.SendAsync(text, token);
    }
}