using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cortiqa.Sdk.Models
{
    public static class ChatRoles
    {
        public const string System = "system";
        public const string User = "user";
        public const string Assistant = "assistant";
        public const string Tool = "tool";
    }

    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = ChatRoles.User;

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        [JsonPropertyName("tool_calls")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ToolCall>? ToolCalls { get; set; }

        [JsonPropertyName("tool_call_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ToolCallId { get; set; }

        public static ChatMessage System(string content) => new ChatMessage { Role = ChatRoles.System, Content = content };
        public static ChatMessage User(string content) => new ChatMessage { Role = ChatRoles.User, Content = content };
        public static ChatMessage Assistant(string content) => new ChatMessage { Role = ChatRoles.Assistant, Content = content };
        public static ChatMessage Tool(string toolCallId, string content) => new ChatMessage
        {
            Role = ChatRoles.Tool,
            ToolCallId = toolCallId,
            Content = content
        };
    }

    public class Tool
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public FunctionDefinition Function { get; set; } = new FunctionDefinition();

        public static Tool FunctionTool(string name, string description, object parameters)
        {
            return new Tool
            {
                Type = "function",
                Function = new FunctionDefinition
                {
                    Name = name,
                    Description = description,
                    Parameters = parameters
                }
            };
        }
    }

    public class FunctionDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Parameters { get; set; }
    }

    public class ToolCall
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public FunctionCall Function { get; set; } = new FunctionCall();
    }

    public class FunctionCall
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = "{}";
    }
}
