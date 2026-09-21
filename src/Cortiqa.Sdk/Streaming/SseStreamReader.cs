using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cortiqa.Sdk.Models;

namespace Cortiqa.Sdk.Streaming
{
    public static class SseStreamReader
    {
        private const string DoneSignal = "[DONE]";
        private const string DataPrefix = "data: ";

        public static async IAsyncEnumerable<ChatCompletionChunk> ReadChunksAsync(
            HttpResponseMessage response,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using (response)
            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(stream))
            {
                string? line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    if (!line.StartsWith(DataPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string data = line.Substring(DataPrefix.Length).Trim();
                    if (data == DoneSignal)
                    {
                        break;
                    }

                    ChatCompletionChunk? chunk = null;
                    try
                    {
                        chunk = JsonSerializer.Deserialize<ChatCompletionChunk>(data);
                    }
                    catch (JsonException)
                    {
                        // Skip malformed chunk
                    }

                    if (chunk != null)
                    {
                        yield return chunk;
                    }
                }
            }
        }

        public static async IAsyncEnumerable<string> ReadTextStreamAsync(
            HttpResponseMessage response,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in ReadChunksAsync(response, cancellationToken).ConfigureAwait(false))
            {
                if (chunk.Choices != null && chunk.Choices.Count > 0)
                {
                    string? text = chunk.Choices[0].Delta?.Content;
                    if (!string.IsNullOrEmpty(text))
                    {
                        yield return text!;
                    }
                }
            }
        }
    }
}
