# Helpers & Advanced Usage in Cortiqa .NET SDK

This guide covers streaming helpers, ASP.NET Core minimal APIs, Polly retries, and Unity optimization.

---

## 1. Streaming Helpers

### `TextStreamAsync`
Returns an `IAsyncEnumerable<string>` that can be consumed with standard C# 8+ `await foreach`:

```csharp
await foreach (var token in client.Messages.TextStreamAsync(request))
{
    Console.Write(token);
}
```

### ASP.NET Core Server-Sent Events (SSE) Endpoint
Stream Falin tokens directly from your backend Web API to browser clients:

```csharp
app.MapGet("/api/chat/stream", async (CortiqaClient client, [FromQuery] string prompt, HttpContext httpContext) =>
{
    httpContext.Response.ContentType = "text/event-stream";

    var request = ChatCompletionRequest.Builder()
        .Model("falin-01")
        .AddMessage(ChatMessage.User(prompt))
        .Build();

    await foreach (var token in client.Messages.TextStreamAsync(request, httpContext.RequestAborted))
    {
        await httpContext.Response.WriteAsync($"data: {token}\n\n");
        await httpContext.Response.Body.FlushAsync();
    }
});
```

---

## 2. Using Custom HttpClient with Polly or SocketsHttpHandler

```csharp
var handler = new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(15),
    MaxConnectionsPerServer = 100
};

var httpClient = new HttpClient(handler)
{
    Timeout = TimeSpan.FromSeconds(60)
};

var client = new CortiqaClient(new CortiqaClientOptions
{
    ApiKey = "sk-cortiqa-...",
    HttpClient = httpClient
});
```
