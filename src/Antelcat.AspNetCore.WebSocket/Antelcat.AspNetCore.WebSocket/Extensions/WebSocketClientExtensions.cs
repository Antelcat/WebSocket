using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Antelcat.AspNetCore.WebSocket.Internals;

namespace Antelcat.AspNetCore.WebSocket.Extensions;

internal static class WebSocketClientExtensions
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

    internal static async Task Echo(
        this Client client,
        System.Net.WebSockets.WebSocket webSocket,
        WebSocketOptions options,
        TaskScheduler? scheduler)
    {
        var cancel = new CancellationTokenSource();
        scheduler ??= new LimitedConcurrencyLevelTaskScheduler(1);
        var factory = new TaskFactory(scheduler);
        while (!webSocket.CloseStatus.HasValue)
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
                    await client.OnDisconnectedAsync(e);
                    goto close;
                }

                var length = result.Count;
                if (result.EndOfMessage)
                {
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await client.OnDisconnectedAsync(result.ToException());
                            goto close;
                        case WebSocketMessageType.Binary:
                            _ = factory.StartNew(
                                () =>
                                {
                                    IList<byte> tmp;
                                    if (data.Count == 0) tmp = new ArraySegment<byte>(buffer, 0, length);
                                    else
                                    {
#if NET8_0_OR_GREATER
                                        data.AddRange(new ReadOnlySpan<byte>(buffer, 0, length));
#else
                                        data.AddRange(buffer.Take(length));
#endif
                                        tmp = data;
                                    }

                                    return client.OnReceivedBytesAsync(tmp, cancel.Token);
                                },
                                cancel.Token);
                            goto next;
                        case WebSocketMessageType.Text:
                            _ = factory.StartNew(
                                () =>
                                {
                                    Span<byte> tmp;
                                    if (data.Count == 0) tmp = new Span<byte>(buffer, 0, length);
                                    else
                                    {
#if NET8_0_OR_GREATER
                                        data.AddRange(new ReadOnlySpan<byte>(buffer, 0, length));
#else
                                        data.AddRange(buffer.Take(length));
#endif
                                        tmp = CollectionsMarshal.AsSpan(data);
                                    }

                                    return client.OnReceivedTextAsync(Encoding.UTF8.GetString(tmp),
                                        cancel.Token);
                                },
                                cancel.Token);
                            goto next;
                    }
                }
                else data.AddRange(buffer);
            }

            next: ;
        }

        close:
#if NET8_0_OR_GREATER
        await cancel.CancelAsync();
#else
        cancel.Cancel();
#endif
        await Task.WhenAll(GetScheduledTasks(scheduler).ToArray());
    }
}