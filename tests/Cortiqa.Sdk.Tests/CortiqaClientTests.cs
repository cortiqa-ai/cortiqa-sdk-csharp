using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cortiqa.Sdk.Exceptions;
using Cortiqa.Sdk.Models;
using Xunit;

namespace Cortiqa.Sdk.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    public class CortiqaClientTests
    {
        [Fact]
        public void ClientInitialization_SetsOptionsCorrectly()
        {
            var options = new CortiqaClientOptions
            {
                ApiKey = "sk-cortiqa-test",
                BaseUrl = "https://api.cortiqa.co",
                MaxRetries = 3
            };

            using var client = new CortiqaClient(options);
            Assert.Equal("sk-cortiqa-test", client.Options.ApiKey);
            Assert.Equal("https://api.cortiqa.co", client.Options.BaseUrl);
            Assert.Equal(3, client.Options.MaxRetries);
            Assert.NotNull(client.Chat);
            Assert.NotNull(client.Messages);
            Assert.NotNull(client.Models);
        }

        [Fact]
        public async Task ChatCompletionsCreate_SendsCorrectHeadersAndParsesResponse()
        {
            string mockResponseBody = @"{
                ""id"": ""chatcmpl-test-123"",
                ""object"": ""chat.completion"",
                ""created"": 1711000000,
                ""model"": ""falin-01"",
                ""choices"": [
                    {
                        ""index"": 0,
                        ""message"": {
                            ""role"": ""assistant"",
                            ""content"": ""Hello from Falin C# SDK!""
                        },
                        ""finish_reason"": ""stop""
                    }
                ],
                ""usage"": {
                    ""prompt_tokens"": 10,
                    ""completion_tokens"": 8,
                    ""total_tokens"": 18
                }
            }";

            var handler = new MockHttpMessageHandler(req =>
            {
                Assert.Equal("Bearer", req.Headers.Authorization?.Scheme);
                Assert.Equal("sk-cortiqa-test", req.Headers.Authorization?.Parameter);
                Assert.True(req.Headers.Contains("User-Agent"));

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(mockResponseBody, System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler);
            var options = new CortiqaClientOptions
            {
                ApiKey = "sk-cortiqa-test",
                HttpClient = httpClient
            };

            using var client = new CortiqaClient(options);

            var request = ChatCompletionRequest.Builder()
                .Model("falin-01")
                .AddMessage(ChatMessage.User("Hi!"))
                .Build();

            var response = await client.Chat.Completions.CreateAsync(request);

            Assert.Equal("chatcmpl-test-123", response.Id);
            Assert.Equal("falin-01", response.Model);
            Assert.Equal("Hello from Falin C# SDK!", response.Content);
            Assert.Equal(18, response.Usage?.TotalTokens);
        }

        [Fact]
        public async Task MessagesCreate_ThrowsAuthenticationException_On401()
        {
            var handler = new MockHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{\"error\":\"unauthorized\"}", System.Text.Encoding.UTF8, "application/json")
                };
            });

            var httpClient = new HttpClient(handler);
            var options = new CortiqaClientOptions
            {
                ApiKey = "bad-key",
                HttpClient = httpClient,
                MaxRetries = 0
            };

            using var client = new CortiqaClient(options);

            var request = ChatCompletionRequest.Builder()
                .Model("falin-01")
                .AddMessage(ChatMessage.User("Hi"))
                .Build();

            await Assert.ThrowsAsync<AuthenticationException>(() => client.Messages.CreateAsync(request));
        }

        [Fact]
        public async Task Streaming_ParsesSseChunksCorrectly()
        {
            string sseContent = "data: {\"id\":\"c-1\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"Hello \"}}]}\n\n" +
                               "data: {\"id\":\"c-2\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"index\":0,\"delta\":{\"content\":\"World!\"}}]}\n\n" +
                               "data: [DONE]\n\n";

            var handler = new MockHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(sseContent, System.Text.Encoding.UTF8, "text/event-stream")
                };
            });

            var httpClient = new HttpClient(handler);
            var options = new CortiqaClientOptions
            {
                ApiKey = "sk-cortiqa-test",
                HttpClient = httpClient
            };

            using var client = new CortiqaClient(options);

            var request = ChatCompletionRequest.Builder()
                .Model("falin-01")
                .AddMessage(ChatMessage.User("Say hello"))
                .Build();

            string accumulated = "";
            await foreach (var token in client.Messages.TextStreamAsync(request))
            {
                accumulated += token;
            }

            Assert.Equal("Hello World!", accumulated);
        }

        [Fact]
        public async Task ChatCompletions_UsesDefaultModel_WhenNotSpecified()
        {
            string? capturedBody = null;
            var handler = new MockHttpMessageHandler(req =>
            {
                capturedBody = req.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""id"": ""chatcmpl-def"",
                        ""choices"": [{""message"": {""role"": ""assistant"", ""content"": ""Default model reply""}}]
                    }", System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new CortiqaClient(new CortiqaClientOptions
            {
                ApiKey = "sk-test",
                HttpClient = new HttpClient(handler)
            });

            var response = await client.Chat.CreateAsync(new ChatCompletionRequest
            {
                Messages = new System.Collections.Generic.List<ChatMessage>
                {
                    ChatMessage.User("Hello")
                }
            });

            Assert.Equal("Default model reply", response.FirstContent);
            Assert.NotNull(capturedBody);
            Assert.Contains("\"openai/gpt-oss-120b\"", capturedBody);
        }

        [Fact]
        public async Task ChatCompletions_ParsesReasoningAndThought()
        {
            var handler = new MockHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""id"": ""chatcmpl-think"",
                        ""choices"": [{
                            ""message"": {
                                ""role"": ""assistant"",
                                ""content"": ""The answer is 42."",
                                ""reasoning"": ""Thinking step by step...""
                            }
                        }]
                    }", System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new CortiqaClient(new CortiqaClientOptions
            {
                ApiKey = "sk-test",
                HttpClient = new HttpClient(handler)
            });

            var response = await client.Chat.CreateAsync(new ChatCompletionRequest
            {
                Messages = new System.Collections.Generic.List<ChatMessage> { ChatMessage.User("What is the meaning?") }
            });

            Assert.Equal("The answer is 42.", response.FirstContent);
            Assert.Equal("Thinking step by step...", response.Reasoning);
            Assert.Equal("Thinking step by step...", response.Choices[0].Message?.Thought);
        }

        [Fact]
        public async Task ChatCompletions_NormalizesToolsPayload()
        {
            string? capturedBody = null;
            var handler = new MockHttpMessageHandler(req =>
            {
                capturedBody = req.Content?.ReadAsStringAsync().Result;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""id"": ""chatcmpl-tools"",
                        ""choices"": [{""message"": {""role"": ""assistant"", ""content"": ""Tool response""}}]
                    }", System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new CortiqaClient(new CortiqaClientOptions
            {
                ApiKey = "sk-test",
                HttpClient = new HttpClient(handler)
            });

            var response = await client.Chat.CreateAsync(new ChatCompletionRequest
            {
                Messages = new System.Collections.Generic.List<ChatMessage> { ChatMessage.User("Use tool") },
                Tools = new System.Collections.Generic.List<Tool>
                {
                    new Tool
                    {
                        Function = new FunctionDefinition { Name = "get_weather", Description = "Get weather" }
                    }
                }
            });

            Assert.NotNull(capturedBody);
            Assert.Contains("\"type\":\"function\"", capturedBody);
            Assert.Contains("\"name\":\"get_weather\"", capturedBody);
        }

        [Fact]
        public async Task PromptAsync_ReturnsAssistantTextDirectly()
        {
            var handler = new MockHttpMessageHandler(req =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""id"": ""chatcmpl-prompt"",
                        ""choices"": [{""message"": {""role"": ""assistant"", ""content"": ""Quick response!""}}]
                    }", System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new CortiqaClient(new CortiqaClientOptions
            {
                ApiKey = "sk-test",
                HttpClient = new HttpClient(handler)
            });

            var reply = await client.PromptAsync("Hello!");
            Assert.Equal("Quick response!", reply);
        }

        [Fact]
        public async Task DetailedError_ParsesParamAndCode_ThrowsUnprocessableEntity()
        {
            var handler = new MockHttpMessageHandler(req =>
            {
                return new HttpResponseMessage((HttpStatusCode)422)
                {
                    Content = new StringContent(@"{
                        ""detail"": [
                            {
                                ""loc"": [""body"", ""messages"", 0, ""content""],
                                ""msg"": ""field required"",
                                ""type"": ""value_error.missing""
                            }
                        ]
                    }", System.Text.Encoding.UTF8, "application/json")
                };
            });

            using var client = new CortiqaClient(new CortiqaClientOptions
            {
                ApiKey = "sk-test",
                HttpClient = new HttpClient(handler),
                MaxRetries = 0
            });

            var ex = await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                client.Chat.CreateAsync(new ChatCompletionRequest()));

            Assert.Equal("messages.0.content", ex.Param);
            Assert.Equal("value_error.missing", ex.Code);
            Assert.Contains("messages.0.content: field required", ex.Message);
        }
    }
}
