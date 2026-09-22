using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cortiqa.Sdk.Exceptions;
using Cortiqa.Sdk.Models;
using Cortiqa.Sdk.Streaming;

namespace Cortiqa.Sdk.Services
{
    public class ChatService
    {
        private readonly CortiqaClient _client;

        public ChatCompletionsService Completions { get; }

        public ChatService(CortiqaClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Completions = new ChatCompletionsService(client);
        }

        public Task<ChatCompletionResponse> CreateAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            return Completions.CreateAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<ChatCompletionChunk> StreamAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            return Completions.StreamAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<string> TextStreamAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            return Completions.TextStreamAsync(request, cancellationToken);
        }
    }

    public class ChatCompletionsService
    {
        private readonly CortiqaClient _client;

        public ChatCompletionsService(CortiqaClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private void NormalizeRequest(ChatCompletionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Model))
            {
                request.Model = _client.Options.DefaultModel;
            }

            if (request.Tools != null)
            {
                foreach (var tool in request.Tools)
                {
                    if (string.IsNullOrWhiteSpace(tool.Type))
                    {
                        tool.Type = "function";
                    }
                }
            }
        }

        public async Task<ChatCompletionResponse> CreateAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Stream = false;
            NormalizeRequest(request);

            string json = JsonSerializer.Serialize(request);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await _client.SendWithRetryAsync(
                    HttpMethod.Post,
                    "/v1/chat/completions",
                    content,
                    cancellationToken).ConfigureAwait(false);

                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseBody);
                return result ?? throw new CortiqaException("Failed to deserialize chat completion response.");
            }
        }

        public async IAsyncEnumerable<ChatCompletionChunk> StreamAsync(
            ChatCompletionRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Stream = true;
            NormalizeRequest(request);

            string json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.SendWithRetryAsync(
                HttpMethod.Post,
                "/v1/chat/completions",
                content,
                cancellationToken,
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            await foreach (var chunk in SseStreamReader.ReadChunksAsync(response, cancellationToken).ConfigureAwait(false))
            {
                yield return chunk;
            }
        }

        public async IAsyncEnumerable<string> TextStreamAsync(
            ChatCompletionRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var chunk in StreamAsync(request, cancellationToken).ConfigureAwait(false))
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
