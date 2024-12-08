using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Antelcat.AspNetCore.WebSocket.Extensions;

namespace Antelcat.AspNetCore.WebSocket;

public class AsyncWebSocketHandler
{
#if NET8_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern IEnumerable<Task> GetScheduledTasks(TaskScheduler scheduler);
#else

    private static readonly MethodInfo MethodGetScheduledTasks =
        typeof(TaskScheduler).GetMethod(nameof(GetScheduledTasks), BindingFlags.NonPublic | BindingFlags.Instance) ??
        throw new MethodAccessException(nameof(GetScheduledTasks));

    private static IEnumerable<Task> GetScheduledTasks(TaskScheduler scheduler) =>
        MethodGetScheduledTasks.Invoke(scheduler, null) as IEnumerable<Task> ?? [];
#endif

    public event Func<Exception?, Task>? Closed;

    public event Func<IList<byte>, CancellationToken, Task>? Binary;

    public event Func<string, CancellationToken, Task>? Text;

    public async Task Handle(System.Net.WebSockets.WebSocket webSocket, WebSocketOptions options)
    {
        var cancel = new CancellationTokenSource();
        while (webSocket.State == WebSocketState.Open)
        {
            var data   = new List<byte>();
            var buffer = new byte[options.ReceiveBufferSize];
            while (true)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await webSocket.ReceiveAsync(buffer, default);
                }
                catch (Exception e)
                {
#if NET8_0_OR_GREATER
                    await cancel.CancelAsync();
#else
                    cancel.Cancel();
#endif
                    Closed?.Invoke(e);
                    return;
                }

                var length = result.Count;
                if (result.EndOfMessage)
                {
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
#if NET8_0_OR_GREATER
                            await cancel.CancelAsync();
#else
                            cancel.Cancel();
#endif
                            Closed?.Invoke(result.ToException());
                            return;
                        case WebSocketMessageType.Binary:
                            IList<byte> bytes;
                            if (data.Count == 0) bytes = new ArraySegment<byte>(buffer, 0, length);
                            else
                            {
#if NET8_0_OR_GREATER
                                data.AddRange(new ReadOnlySpan<byte>(buffer, 0, length));
#else
                                data.AddRange(buffer.Take(length));
#endif
                                bytes = data;
                            }

                            Binary?.Invoke(bytes, cancel.Token);

                            goto next;
                        case WebSocketMessageType.Text:
                            string str;
                            if (data.Count == 0) str = Encoding.UTF8.GetString(new Span<byte>(buffer, 0, length));
                            else
                            {
#if NET8_0_OR_GREATER
                                data.AddRange(new ReadOnlySpan<byte>(buffer, 0, length));
#else
                                data.AddRange(buffer.Take(length));
#endif
                                str = Encoding.UTF8.GetString(CollectionsMarshal.AsSpan(data));
                            }

                            Text?.Invoke(str, cancel.Token);
                            goto next;
                    }
                }
                else data.AddRange(buffer);
            }

            next: ;
        }
    }
}