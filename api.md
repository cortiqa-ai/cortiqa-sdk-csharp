# Cortiqa .NET / C# SDK API Reference

Comprehensive reference for all namespaces, classes, methods, and configurations in `Cortiqa.Sdk`.

---

## Table of Contents

- [Client Initialization](#client-initialization)
- [Services](#services)
  - [`client.Messages`](#clientmessages)
  - [`client.Chat.Completions`](#clientchatcompletions)
  - [`client.Models`](#clientmodels)
- [Data Models](#data-models)
- [Streaming Architecture](#streaming-architecture)
- [Exception Handling](#exception-handling)

---

## Client Initialization

```csharp
using Cortiqa.Sdk;

// 1. Automatically load from CORTIQA_API_KEY environment variable
using var client = CortiqaClient.FromEnvironment();

// 2. Explicit API key
using var client = new CortiqaClient("sk-cortiqa-your-api-key");

// 3. Full options configuration
using var client = new CortiqaClient(new CortiqaClientOptions
{
    ApiKey = "sk-cortiqa-your-api-key",
    BaseUrl = "https://api.cortiqa.co",
    Timeout = TimeSpan.FromSeconds(45),
    MaxRetries = 3,
    HttpClient = customHttpClient // Optional custom HttpClient
});
```

---

## Services

### `client.Messages`
Anthropic-style messaging service.

#### `client.Messages.CreateAsync(request, cancellationToken)`
Sends a synchronous completion request and returns `Task<ChatCompletionResponse>`.

```csharp
var response = await client.Messages.CreateAsync(
    ChatCompletionRequest.Builder()
        .Model("falin-01")
        .AddMessage(ChatMessage.User("Hello Falin!"))
        .Temperature(0.7)
        .MaxTokens(500)
        .Build()
);

Console.WriteLine(response.Content);
```

#### `client.Messages.TextStreamAsync(request, cancellationToken)`
Yields an `IAsyncEnumerable<string>` of response text tokens in real time.

```csharp
await foreach (string token in client.Messages.TextStreamAsync(request))
{
    Console.Write(token);
}
```

#### `client.Messages.StreamAsync(request, cancellationToken)`
Yields raw `ChatCompletionChunk` instances for advanced delta inspection.

---

### `client.Chat.Completions`
OpenAI-compatible chat completion service.

```csharp
var response = await client.Chat.Completions.CreateAsync(request);
await foreach (var token in client.Chat.Completions.TextStreamAsync(request))
{
    Console.Write(token);
}
```

---

### `client.Models`
List available Cortiqa AI foundation models.

```csharp
var models = await client.Models.ListAsync();
foreach (var model in models.Data)
{
    Console.WriteLine($"{model.Id}: {model.Description}");
}
```

---

## Exception Handling

All exceptions inherit from `Cortiqa.Sdk.Exceptions.CortiqaException`:

```csharp
try
{
    var response = await client.Messages.CreateAsync(request);
}
catch (AuthenticationException ex)
{
    Console.Error.WriteLine("Invalid API Key: " + ex.Message);
}
catch (RateLimitException ex)
{
    Console.Error.WriteLine("Rate limit exceeded: " + ex.Message);
}
catch (ApiException ex)
{
    Console.Error.WriteLine($"API Error {(int)ex.StatusCode}: {ex.ResponseBody}");
}
```
