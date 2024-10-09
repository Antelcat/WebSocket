using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Antelcat.AspNetCore.WebSocket;
using Antelcat.AspNetCore.WebSocket.Extensions;
using Antelcat.AspNetCore.WebSocket.Internals;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class WebSocketExtensions
{
    [RequiresUnreferencedCode(
        "This method dynamically search for types which is sub-class of Client in entry assembly")]
    public static IServiceCollection AddWebSockets(this IServiceCollection collection)
    {
        foreach (var type in Assembly.GetEntryAssembly()?
                .GetExportedTypes()
                .Where(static x => x.IsSubclassOf(typeof(Client))) ?? [])
        {
            collection.AddTransient(type);
        }

        return collection;
    }

    /// <summary>
    /// <see cref="TClient"/> should be add to <see cref="IServiceCollection"/> before <see cref="IApplicationBuilder.Build()"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="pattern"></param>
    /// <param name="options"></param>
    /// <param name="scheduler"></param>
    /// <typeparam name="TClient"></typeparam>
    /// <returns></returns>
    public static
#if NET8_0_OR_GREATER
        RouteHandlerBuilder
#else
        IEndpointConventionBuilder
#endif
        MapWebSocket<TClient>(
            this IEndpointRouteBuilder builder,
            [StringSyntax("Route")] string pattern,
            Antelcat.AspNetCore.WebSocket.WebSocketOptions? options = null,
            Func<TaskScheduler>? scheduler = null)
        where TClient : Client
    {
        return builder.Map(pattern,
#if NET8_0_OR_GREATER
            (Func<HttpContext,Task>)
#else
            (RequestDelegate)
#endif
            (async context =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
#if NET6_0_OR_GREATER
                    await Results.BadRequest().ExecuteAsync(context);
#endif
                    return;
                }

                var       client    = context.RequestServices.GetRequiredService<TClient>();
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                client.Context = new WebSocketCallerContext(context, webSocket);
                await client.OnConnectedAsync();
                await client.Echo(webSocket,
                    options ?? new Antelcat.AspNetCore.WebSocket.WebSocketOptions(),
                    scheduler?.Invoke());
            }));
    }

    
}