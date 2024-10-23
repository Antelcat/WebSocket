namespace Antelcat.AspNetCore.WebSocket.Internals;

internal static class WebSocketClientEcho
{
    internal static async Task Handle(
        this Client client,
        System.Net.WebSockets.WebSocket webSocket,
        WebSocketOptions options)
    {
        var handler = new AsyncWebSocketHandler();
        handler.Text   += client.OnReceivedTextAsync;
        handler.Binary += client.OnReceivedBytesAsync;
        handler.Closed += client.OnDisconnectedAsync;
        await handler.Handle(webSocket, options);
    }
}