using System.Net.WebSockets;
using System.Reflection;
using Antelcat.AutoGen.ComponentModel.Diagnostic;

namespace Antelcat.AspNetCore.WebSocket;

[Serializable]
[AutoMetadataFrom(typeof(WebSocketCloseStatus), MemberTypes.Field,
    Leading = """
              /// <summary>
              /// </summary>
              /// <param name="exception"> One of
              """,
    Template = """
               
               /// <p><see cref="Antelcat.AspNetCore.WebSocket.Exceptions.{Name}Exception"/></p>
               """,
    Trailing = """
               
               /// </param>
               /// <returns></returns>
               protected internal virtual partial Task OnDisconnectedAsync(Exception? exception);
               """
)]
public abstract partial class Client : IDisposable
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

    
    protected internal virtual partial Task OnDisconnectedAsync(Exception? exception) => Task.CompletedTask;

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