using System.Net.WebSockets;
using System.Reflection;
using Antelcat.AutoGen.ComponentModel.Diagnostic;

[assembly: AutoMetadataFrom(typeof(WebSocketCloseStatus), MemberTypes.Field,
    Leading = "namespace Antelcat.AspNetCore.WebSocket.Exceptions;",
    Template = """
               ///<summary>
               /// <inheritdoc cref="System.Net.WebSockets.WebSocketCloseStatus.{Name}"/>
               ///</summary>
               public class {Name}Exception(string? message) : WebSocketException(System.Net.WebSockets.WebSocketCloseStatus.{Name}, message);
               """
)]

namespace Antelcat.AspNetCore.WebSocket.Extensions;

[AutoMetadataFrom(typeof(WebSocketCloseStatus), MemberTypes.Field,
    Leading = """
              public static Exception? ToException(
                this System.Net.WebSockets.WebSocketReceiveResult result)
                => result.CloseStatus is null
              ? null
              : result.CloseStatus.Value switch
              {
              """,
    Template = """
                System.Net.WebSockets.WebSocketCloseStatus.{Name} => 
                new Antelcat.AspNetCore.WebSocket.Exceptions.{Name}Exception(result.CloseStatusDescription),
               """,
    Trailing = """
               _ => new Antelcat.AspNetCore.WebSocket.Exceptions.CustomWebSocketException(result.CloseStatus.Value ,result.CloseStatusDescription) };
               """
)]
public static partial class ExceptionExtensions;
