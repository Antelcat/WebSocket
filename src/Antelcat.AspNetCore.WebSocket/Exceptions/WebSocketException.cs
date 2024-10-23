using System.Net.WebSockets;

namespace Antelcat.AspNetCore.WebSocket.Exceptions;

public abstract class WebSocketException(WebSocketCloseStatus closeStatus, string? message) : Exception(message)
{
    public WebSocketCloseStatus CloseStatus { get; } = closeStatus;
}