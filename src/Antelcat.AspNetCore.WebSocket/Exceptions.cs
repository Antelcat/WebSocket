using System.Net.WebSockets;
using System.Reflection;
using Antelcat.AutoGen.ComponentModel.Diagnostic;

namespace Antelcat.AspNetCore.WebSocket;

[AutoMetadataFrom(typeof(WebSocketCloseStatus), MemberTypes.Field,
    Template = """
               ///<summary>
               /// <inheritdoc cref="System.Net.WebSockets.WebSocketCloseStatus.{Name}"/>
               ///</summary>
               public class {Name}Exception(string? message) : Exception(message);
               """
)]
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
                new {Name}Exception(result.CloseStatusDescription),
               """,
    Trailing = """
               _ => null };
               """
)]
public static partial class Exceptions;