# Changelog

All notable changes to `Cortiqa.Sdk` will be documented in this file.

---

## [0.1.0] - 2026-09-21

### Added
- Initial release of the official Cortiqa C# / .NET SDK (`Cortiqa.Sdk`).
- Multi-targeted for `.NET 8.0` and `.NET Standard 2.0` (compatible with Unity 2021+).
- Dual API interface:
  - Anthropic-style: `client.Messages.CreateAsync` and `client.Messages.TextStreamAsync`.
  - OpenAI-style: `client.Chat.Completions.CreateAsync` and `client.Chat.Completions.StreamAsync`.
- Support for Falin Foundation Models (`falin-01`, `falin-pro`, `falin-vision`, `falin-ultra`).
- Structured Tool Calling (Function Calling).
- Real-time token streaming via `IAsyncEnumerable<string>` and `IAsyncEnumerable<ChatCompletionChunk>`.
- Automatic exponential backoff retries for 429 rate limits and 5xx server errors.
- Built-in ASP.NET Core dependency injection compatibility.
- Comprehensive exception hierarchy (`AuthenticationException`, `RateLimitException`, `ApiException`).
