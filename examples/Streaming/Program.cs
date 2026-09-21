using System;
using System.Threading.Tasks;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

namespace StreamingExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = CortiqaClient.FromEnvironment();

            Console.WriteLine("Streaming token response from Falin-01:\n");

            var request = ChatCompletionRequest.Builder()
                .Model("falin-01")
                .AddMessage(ChatMessage.User("Write a fast C# method to compute Fibonacci numbers."))
                .Build();

            // Real-time asynchronous token streaming via IAsyncEnumerable<string>
            await foreach (var token in client.Messages.TextStreamAsync(request))
            {
                Console.Write(token);
            }

            Console.WriteLine("\n\n[Stream Complete]");
        }
    }
}
