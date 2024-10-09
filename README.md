# WebSocket
 .NET wrap for WebSocket

# Example

in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

...
builder.Services.AddWebSockets();
...
    
var app = builder.Build();
...
// remember to call UseWebSockets() before MapWebSocket()
app.UseWebSockets(); 
app.MapWebSocket<SampleWebSocketClient>("/WebSocket");
...
```

for `SampleWebSocketClient`

which lifetime is `Transient` or `Scoped`, each WebSocket connection will create corresponding instance

```csharp
// define your client model to receive message from WebSocket client 
public class SampleWebSocketClient : Antelcat.AspNetCore.Client
{
    protected override async Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
    }

    protected override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"{Context.ConnectionId} disconnected due to {exception}");
    }

    protected override async Task OnReceivedTextAsync(string text, CancellationToken token)
    {
        await this.SendAsync(text, token);
    }
}
```