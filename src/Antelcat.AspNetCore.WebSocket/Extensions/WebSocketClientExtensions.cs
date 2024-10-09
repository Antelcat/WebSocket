using System.Net.WebSockets;
using System.Text;

namespace Antelcat.AspNetCore.WebSocket.Extensions;

public static class WebSocketClientExtensions
{
    /// <summary>
    /// Send message using <see cref="Encoding.UTF8"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Task SendAsync(this Client context, string text) =>
        context.SendAsync(text, CancellationToken.None);

    /// <summary>
    /// Send message using <see cref="Encoding.UTF8"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="text"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task SendAsync(this Client context,
                                 string text,
                                 CancellationToken cancellationToken) =>
        context.SendAsync(text, Encoding.UTF8, cancellationToken);

    public static Task SendAsync(this Client context,
                                 string text,
                                 Encoding encoding,
                                 CancellationToken cancellationToken = default) =>
        context.SendAsync(encoding.GetBytes(text), WebSocketMessageType.Text, true, cancellationToken);
}