# Tool Calling (Function Calling) with Cortiqa .NET SDK

Cortiqa models (`falin-01`, `falin-pro`, `falin-ultra`) support structured tool calling for invoking C# methods, querying SQL databases, and building autonomous agent loops.

---

## Defining a Tool

Use `Tool.FunctionTool(name, description, parameters)`:

```csharp
using Cortiqa.Sdk.Models;

var databaseTool = Tool.FunctionTool(
    name: "query_database",
    description: "Search orders by customer email or ID",
    parameters: new
    {
        type = "object",
        properties = new
        {
            query = new { type = "string", description = "Customer email or ID" }
        },
        required = new[] { "query" }
    }
);
```

---

## Autonomous Tool Calling Loop

```csharp
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

using var client = CortiqaClient.FromEnvironment();

var conversation = new List<ChatMessage>
{
    ChatMessage.User("Can you check the status of order #98231?")
};

var response = await client.Messages.CreateAsync(
    ChatCompletionRequest.Builder()
        .Model("falin-01")
        .Messages(conversation)
        .Tools(new List<Tool> { databaseTool })
        .Build()
);

var choice = response.Choices[0];
conversation.Add(choice.Message);

if (choice.Message.ToolCalls != null && choice.Message.ToolCalls.Count > 0)
{
    foreach (var toolCall in choice.Message.ToolCalls)
    {
        if (toolCall.Function.Name == "query_database")
        {
            // Execute local C# service
            string toolResult = "{\"orderId\": \"98231\", \"status\": \"Shipped\", \"carrier\": \"FedEx\"}";

            conversation.Add(ChatMessage.Tool(toolCall.Id, toolResult));
        }
    }

    var finalResponse = await client.Messages.CreateAsync(
        ChatCompletionRequest.Builder()
            .Model("falin-01")
            .Messages(conversation)
            .Build()
    );

    Console.WriteLine("Final Answer: " + finalResponse.Content);
}
```
