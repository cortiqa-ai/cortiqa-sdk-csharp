using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cortiqa.Sdk.Models;

namespace Cortiqa.Sdk.Services
{
    /// <summary>
    /// Anthropic-style messages service for chat completion and token streaming.
    /// </summary>
    public class MessagesService
    {
        private readonly CortiqaClient _client;

        public MessagesService(CortiqaClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<ChatCompletionResponse> CreateAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            return _client.Chat.Completions.CreateAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<ChatCompletionChunk> StreamAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            return _client.Chat.Completions.StreamAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<string> TextStreamAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
        {
            return _client.Chat.Completions.TextStreamAsync(request, cancellationToken);
        }
    }
}
