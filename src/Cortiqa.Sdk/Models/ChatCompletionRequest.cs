using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cortiqa.Sdk.Models
{
    public class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = CortiqaClientOptions.DefaultModelId;

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("top_p")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? TopP { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("tools")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Tool>? Tools { get; set; }

        [JsonPropertyName("tool_choice")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? ToolChoice { get; set; }

        public static ChatCompletionRequestBuilder Builder() => new ChatCompletionRequestBuilder();
    }

    public class ChatCompletionRequestBuilder
    {
        private readonly ChatCompletionRequest _request = new ChatCompletionRequest();

        public ChatCompletionRequestBuilder Model(string model)
        {
            _request.Model = model;
            return this;
        }

        public ChatCompletionRequestBuilder Messages(List<ChatMessage> messages)
        {
            _request.Messages = messages;
            return this;
        }

        public ChatCompletionRequestBuilder AddMessage(ChatMessage message)
        {
            _request.Messages.Add(message);
            return this;
        }

        public ChatCompletionRequestBuilder Temperature(double temperature)
        {
            _request.Temperature = temperature;
            return this;
        }

        public ChatCompletionRequestBuilder MaxTokens(int maxTokens)
        {
            _request.MaxTokens = maxTokens;
            return this;
        }

        public ChatCompletionRequestBuilder TopP(double topP)
        {
            _request.TopP = topP;
            return this;
        }

        public ChatCompletionRequestBuilder Stream(bool stream = true)
        {
            _request.Stream = stream;
            return this;
        }

        public ChatCompletionRequestBuilder Tools(List<Tool> tools)
        {
            _request.Tools = tools;
            return this;
        }

        public ChatCompletionRequestBuilder ToolChoice(object toolChoice)
        {
            _request.ToolChoice = toolChoice;
            return this;
        }

        public ChatCompletionRequest Build() => _request;
    }
}
