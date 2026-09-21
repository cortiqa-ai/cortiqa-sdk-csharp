using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

namespace ToolsExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = CortiqaClient.FromEnvironment();

            // 1. Declare tool schema
            var weatherTool = Tool.FunctionTool(
                name: "get_weather",
                description: "Get temperature for a city",
                parameters: new
                {
                    type = "object",
                    properties = new
                    {
                        city = new { type = "string", description = "Name of city, e.g. Seattle, Mumbai" },
                        unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
                    },
                    required = new[] { "city" }
                }
            );

            var conversation = new List<ChatMessage>
            {
                ChatMessage.User("What's the weather in Mumbai?")
            };

            Console.WriteLine("User: What's the weather in Mumbai?");

            // 2. Request completion with available tools
            var response = await client.Messages.CreateAsync(
                ChatCompletionRequest.Builder()
                    .Model("falin-01")
                    .Messages(conversation)
                    .Tools(new List<Tool> { weatherTool })
                    .Build()
            );

            var choice = response.Choices[0];
            conversation.Add(choice.Message);

            // 3. Inspect tool calls
            if (choice.Message.ToolCalls != null && choice.Message.ToolCalls.Count > 0)
            {
                var toolCall = choice.Message.ToolCalls[0];
                Console.WriteLine($"\n[Model invoked tool: {toolCall.Function.Name} with args: {toolCall.Function.Arguments}]");

                // Execute local function
                string result = "{\"city\": \"Mumbai\", \"temp\": \"31C\", \"condition\": \"Partly Cloudy\"}";

                // 4. Append tool result
                conversation.Add(ChatMessage.Tool(toolCall.Id, result));

                // 5. Send back to model
                var finalResponse = await client.Messages.CreateAsync(
                    ChatCompletionRequest.Builder()
                        .Model("falin-01")
                        .Messages(conversation)
                        .Build()
                );

                Console.WriteLine("\nFinal Assistant Answer:\n" + finalResponse.Content);
            }
        }
    }
}
