# Cortiqa AI .NET / C# SDK

[![NuGet Version](https://img.shields.io/nuget/v/Cortiqa.Sdk.svg)](https://www.nuget.org/packages/Cortiqa.Sdk/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Target Frameworks](https://img.shields.io/badge/.NET-8.0%20%7C%20Standard%202.0-purple.svg)](https://dotnet.microsoft.com/)

Official C# / .NET client library for **Cortiqa AI** and **Falin Foundation Models**. Built for enterprise backend microservices, ASP.NET Core, Windows desktop apps (WPF/WinUI), and Unity AI game development.

---

## ⚡ Installation

### NuGet Package Manager
```bash
dotnet add package Cortiqa.Sdk
```

### Package Manager Console (Visual Studio)
```powershell
Install-Package Cortiqa.Sdk
```

---

## 🚀 Quickstart

Set your API key in your environment:

```bash
export CORTIQA_API_KEY="sk-cortiqa-your-api-key"
```

Then create a chat completion:

```csharp
using System;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

// Automatically reads from CORTIQA_API_KEY
using var client = CortiqaClient.FromEnvironment();

var response = await client.Messages.CreateAsync(
    ChatCompletionRequest.Builder()
        .Model("falin-01")
        .AddMessage(ChatMessage.User("Explain dependency injection in C#."))
        .Build()
);

Console.WriteLine(response.Content);
```

---

## 🌊 Real-Time Token Streaming

Stream tokens asynchronously using native `IAsyncEnumerable<string>`:

```csharp
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

using var client = CortiqaClient.FromEnvironment();

var request = ChatCompletionRequest.Builder()
    .Model("falin-01")
    .AddMessage(ChatMessage.User("Write a fast sorting algorithm in C#."))
    .Build();

await foreach (var token in client.Messages.TextStreamAsync(request))
{
    Console.Write(token);
}
```

---

## 🛠️ Tool Calling (Function Calling)

```csharp
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

using var client = CortiqaClient.FromEnvironment();

var weatherTool = Tool.FunctionTool(
    name: "get_weather",
    description: "Get temperature for a city",
    parameters: new
    {
        type = "object",
        properties = new { city = new { type = "string" } },
        required = new[] { "city" }
    }
);

var response = await client.Messages.CreateAsync(
    ChatCompletionRequest.Builder()
        .Model("falin-01")
        .AddMessage(ChatMessage.User("What's the weather in Bengaluru?"))
        .Tools(new List<Tool> { weatherTool })
        .Build()
);

var toolCall = response.Choices[0].Message.ToolCalls?[0];
if (toolCall != null)
{
    Console.WriteLine($"Model requested: {toolCall.Function.Name} with args: {toolCall.Function.Arguments}");
}
```

---

## 🌐 ASP.NET Core Dependency Injection

Register `CortiqaClient` in `Program.cs`:

```csharp
// Program.cs
builder.Services.AddSingleton<CortiqaClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new CortiqaClient(new CortiqaClientOptions
    {
        ApiKey = config["Cortiqa:ApiKey"],
        Timeout = TimeSpan.FromSeconds(30),
        MaxRetries = 3
    });
});
```

And inject it into controllers or minimal API endpoints:

```csharp
app.MapPost("/api/chat", async (CortiqaClient client, [FromBody] ChatRequest req) =>
{
    var response = await client.Messages.CreateAsync(
        ChatCompletionRequest.Builder()
            .Model("falin-01")
            .AddMessage(ChatMessage.User(req.Prompt))
            .Build()
    );
    return Results.Ok(new { answer = response.Content });
});
```

---

## 🎮 Unity AI Game Development

Works out of the box in Unity 2021+ with `.NET Standard 2.0`:

```csharp
using UnityEngine;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

public class NpcDialogue : MonoBehaviour
{
    private CortiqaClient client = new CortiqaClient("sk-cortiqa-...");

    public async void OnPlayerInteract(string query)
    {
        var response = await client.Messages.CreateAsync(
            ChatCompletionRequest.Builder()
                .Model("falin-01")
                .AddMessage(ChatMessage.System("You are a castle guard."))
                .AddMessage(ChatMessage.User(query))
                .Build()
        );
        Debug.Log("Guard: " + response.Content);
    }
}
```

---

## 🤖 Models Supported

| Model ID | Best For | Context Window |
|---|---|---|
| `falin-01` | Flagship fast reasoning & coding | 128k |
| `falin-pro` | Complex software architecture & math | 200k |
| `falin-vision` | Multimodal image understanding & OCR | 128k |
| `falin-ultra` | Deep enterprise research | 256k |

---

## 📄 License

MIT © [Cortiqa AI](https://cortiqa.co)
