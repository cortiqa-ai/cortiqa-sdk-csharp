using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cortiqa.Sdk.Models
{
    public class ChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = "chat.completion";

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice> Choices { get; set; } = new List<ChatCompletionChoice>();

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }

        /// <summary>
        /// Convenience property returning the first choice assistant text content.
        /// </summary>
        [JsonIgnore]
        public string Content => (Choices != null && Choices.Count > 0) ? Choices[0].Message?.Content ?? string.Empty : string.Empty;
    }

    public class ChatCompletionChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public ChatMessage Message { get; set; } = new ChatMessage();

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class ChatCompletionChunk
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = "chat.completion.chunk";

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<ChatCompletionChunkChoice> Choices { get; set; } = new List<ChatCompletionChunkChoice>();
    }

    public class ChatCompletionChunkChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("delta")]
        public ChatChunkDelta Delta { get; set; } = new ChatChunkDelta();

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class ChatChunkDelta
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<ToolCall>? ToolCalls { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class ModelListResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = "list";

        [JsonPropertyName("data")]
        public List<ModelInfo> Data { get; set; } = new List<ModelInfo>();
    }

    public class ModelInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = "model";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("context_window")]
        public int? ContextWindow { get; set; }

        [JsonPropertyName("max_output_tokens")]
        public int? MaxOutputTokens { get; set; }
    }
}
