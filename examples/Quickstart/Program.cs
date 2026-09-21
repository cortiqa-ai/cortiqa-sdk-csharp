using System;
using System.Threading.Tasks;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

namespace QuickstartExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialized from CORTIQA_API_KEY environment variable
            using var client = CortiqaClient.FromEnvironment();

            Console.WriteLine("Sending chat completion request to Falin-01...");

            var response = await client.Messages.CreateAsync(
                ChatCompletionRequest.Builder()
                    .Model("falin-01")
                    .AddMessage(ChatMessage.User("Explain dependency injection in C# in 2 sentences."))
                    .Temperature(0.7)
                    .Build()
            );

            Console.WriteLine("\nResponse:");
            Console.WriteLine(response.Content);
            Console.WriteLine($"\nTokens Used: {response.Usage?.TotalTokens}");
        }
    }
}
