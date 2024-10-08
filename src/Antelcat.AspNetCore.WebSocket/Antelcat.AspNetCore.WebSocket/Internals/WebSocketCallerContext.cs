namespace Antelcat.AspNetCore.WebSocket.Internals;

internal class WebSocketCallerContext(HttpContext httpContext, System.Net.WebSockets.WebSocket webSocket)
    : ClientCallerContext
{
    internal override System.Net.WebSockets.WebSocket WebSocket    => webSocket;
    public override   HttpContext                     HttpContext  => httpContext;
    public override   string                          ConnectionId => httpContext.Connection.Id;

    public override void Abort()
    {
        WebSocket.Abort();
    }
}