using System.Net.WebSockets;

namespace Antelcat.AspNetCore.WebSocket.Exceptions;

public class CustomWebSocketException(WebSocketCloseStatus closeStatus, string? message)
    : WebSocketException(closeStatus, message);
