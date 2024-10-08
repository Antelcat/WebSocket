namespace Antelcat.AspNetCore.WebSocket;

public abstract class ClientCallerContext
{
    internal abstract System.Net.WebSockets.WebSocket WebSocket { get; }
    
    public abstract HttpContext HttpContext { get; }

    public abstract string ConnectionId { get; }

    public abstract void Abort();
}