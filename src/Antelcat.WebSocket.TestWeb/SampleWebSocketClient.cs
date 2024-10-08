using Antelcat.AspNetCore.WebSocket;

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
}