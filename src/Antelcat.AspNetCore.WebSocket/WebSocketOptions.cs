namespace Antelcat.AspNetCore.WebSocket;

public class WebSocketOptions
{
    /// <summary>
    /// Gets or sets the size of the protocol buffer used to receive and parse frames. The default is 4kb.
    /// </summary>
    public int ReceiveBufferSize { get; set; } = 1024 * 4;
}