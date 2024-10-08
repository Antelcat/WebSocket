using System.Net.WebSockets;

namespace Antelcat.AspNetCore.WebSocket;

[Serializable]
public abstract class Client : IDisposable
{
    private bool disposed;
    
    protected internal ClientCallerContext Context { get; internal set; } = default!;

    ///<inheritdoc cref="WebSocket.SendAsync(ArraySegment{byte},WebSocketMessageType,bool,CancellationToken)"/>
    public Task SendAsync(
        ArraySegment<byte> buffer,
        WebSocketMessageType messageType,
        bool endOfMessage,
        CancellationToken cancellationToken) =>
        Context.WebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

    ///<inheritdoc cref="WebSocket.SendAsync(ReadOnlyMemory{byte},WebSocketMessageType,bool,CancellationToken)"/>
    public ValueTask SendAsync(
        ReadOnlyMemory<byte> buffer,
        WebSocketMessageType messageType,
        bool endOfMessage,
        CancellationToken cancellationToken) =>
        Context.WebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

#if NET6_0_OR_GREATER
    ///<inheritdoc cref="WebSocket.SendAsync(ReadOnlyMemory{byte},WebSocketMessageType,WebSocketMessageFlags,CancellationToken)"/>
    public ValueTask SendAsync(
        ReadOnlyMemory<byte> buffer,
        WebSocketMessageType messageType,
        WebSocketMessageFlags messageFlags,
        CancellationToken cancellationToken = default) =>
        Context.WebSocket.SendAsync(buffer, messageType, messageFlags, cancellationToken);
#endif

    protected internal virtual Task OnConnectedAsync() => Task.CompletedTask;

    protected internal virtual Task OnReceivedBytesAsync(IList<byte> data, CancellationToken token) =>
        Task.CompletedTask;

    protected internal virtual Task OnReceivedTextAsync(string text, CancellationToken token) => Task.CompletedTask;

    /// <summary>
    /// </summary>
    /// <param name="exception"><see cref="Exceptions"/></param>
    /// <returns></returns>
    protected internal virtual Task OnDisconnectedAsync(Exception? exception) => Task.CompletedTask;

    public void Dispose()
    {
        if (disposed) return;
        Dispose(true);
        disposed = true;
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}