using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Cortiqa.Sdk.Exceptions;
using Cortiqa.Sdk.Services;

namespace Cortiqa.Sdk
{
    /// <summary>
    /// Official client for Cortiqa AI and Falin Foundation Models.
    /// </summary>
    public class CortiqaClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;
        private readonly CortiqaClientOptions _options;

        public const string Version = "0.1.1";

        /// <summary>
        /// OpenAI-compatible chat service.
        /// </summary>
        public ChatService Chat { get; }

        /// <summary>
        /// Anthropic-style messages service.
        /// </summary>
        public MessagesService Messages { get; }

        /// <summary>
        /// Models service for querying available foundation models.
        /// </summary>
        public ModelsService Models { get; }

        public CortiqaClientOptions Options => _options;

        public CortiqaClient(CortiqaClientOptions? options = null)
        {
            _options = options ?? CortiqaClientOptions.FromEnvironment();

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _options.ApiKey = Environment.GetEnvironmentVariable("CORTIQA_API_KEY");
            }

            if (_options.HttpClient != null)
            {
                _httpClient = _options.HttpClient;
                _disposeHttpClient = false;
            }
            else
            {
                _httpClient = new HttpClient { Timeout = _options.Timeout };
                _disposeHttpClient = true;
            }

            Chat = new ChatService(this);
            Messages = new MessagesService(this);
            Models = new ModelsService(this);
        }

        public CortiqaClient(string apiKey) : this(new CortiqaClientOptions { ApiKey = apiKey })
        {
        }

        public static CortiqaClient FromEnvironment() => new CortiqaClient(CortiqaClientOptions.FromEnvironment());

        /// <summary>
        /// Quick one-liner helper to generate a chat completion and return the assistant text directly.
        /// </summary>
        /// <param name="prompt">User message prompt.</param>
        /// <param name="system">Optional system prompt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Assistant response text.</returns>
        public async Task<string> PromptAsync(string prompt, string? system = null, CancellationToken cancellationToken = default)
        {
            var messages = new System.Collections.Generic.List<Cortiqa.Sdk.Models.ChatMessage>();
            if (!string.IsNullOrWhiteSpace(system))
            {
                messages.Add(Cortiqa.Sdk.Models.ChatMessage.System(system!));
            }
            messages.Add(Cortiqa.Sdk.Models.ChatMessage.User(prompt));

            var response = await Chat.CreateAsync(new Cortiqa.Sdk.Models.ChatCompletionRequest
            {
                Messages = messages
            }, cancellationToken).ConfigureAwait(false);

            return response.Content;
        }

        internal async Task<HttpResponseMessage> SendWithRetryAsync(
            HttpMethod method,
            string relativePath,
            HttpContent? content = null,
            CancellationToken cancellationToken = default,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            int attempts = 0;
            int maxAttempts = Math.Max(1, _options.MaxRetries + 1);
            TimeSpan delay = TimeSpan.FromMilliseconds(500);

            string baseUrl = _options.BaseUrl.TrimEnd('/');
            string url = relativePath.StartsWith("/") ? baseUrl + relativePath : baseUrl + "/" + relativePath;

            while (true)
            {
                attempts++;
                var request = new HttpRequestMessage(method, url);

                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
                }

                request.Headers.Add("User-Agent", $"cortiqa-sdk-csharp/{Version}");

                if (content != null)
                {
                    request.Content = content;
                }

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException) when (attempts < maxAttempts && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                    continue;
                }

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                int statusCode = (int)response.StatusCode;
                bool isRetryable = (statusCode == 429 || statusCode >= 500);

                if (isRetryable && attempts < maxAttempts)
                {
                    response.Dispose();
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                    continue;
                }

                string? errorBody = null;
                try
                {
                    errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch
                {
                    // Ignore failure to read error body
                }

                response.Dispose();

                string message = $"Cortiqa API error (HTTP {statusCode}).";
                string? param = null;
                string? code = null;
                string? errorType = null;

                if (!string.IsNullOrWhiteSpace(errorBody))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(errorBody!);
                        var root = doc.RootElement;
                        if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("error", out var errorProp))
                            {
                                if (errorProp.ValueKind == System.Text.Json.JsonValueKind.Object)
                                {
                                    if (errorProp.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == System.Text.Json.JsonValueKind.String)
                                        message = msgProp.GetString() ?? message;
                                    if (errorProp.TryGetProperty("param", out var paramProp) && paramProp.ValueKind == System.Text.Json.JsonValueKind.String)
                                        param = paramProp.GetString();
                                    if (errorProp.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == System.Text.Json.JsonValueKind.String)
                                        code = codeProp.GetString();
                                    if (errorProp.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == System.Text.Json.JsonValueKind.String)
                                        errorType = typeProp.GetString();
                                }
                                else if (errorProp.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    message = errorProp.GetString() ?? message;
                                }
                            }
                            else if (root.TryGetProperty("detail", out var detailProp))
                            {
                                if (detailProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                                {
                                    var detailItems = new System.Collections.Generic.List<string>();
                                    bool isFirst = true;
                                    foreach (var item in detailProp.EnumerateArray())
                                    {
                                        string? locStr = null;
                                        if (item.TryGetProperty("loc", out var locProp) && locProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                                        {
                                            var locParts = new System.Collections.Generic.List<string>();
                                            foreach (var p in locProp.EnumerateArray())
                                            {
                                                var pStr = p.ToString();
                                                if (pStr != "body") locParts.Add(pStr);
                                            }
                                            if (locParts.Count > 0)
                                            {
                                                locStr = string.Join(".", locParts);
                                                if (isFirst) param = locStr;
                                            }
                                        }

                                        string? msg = null;
                                        if (item.TryGetProperty("msg", out var m) && m.ValueKind == System.Text.Json.JsonValueKind.String)
                                        {
                                            msg = m.GetString();
                                        }

                                        if (isFirst && item.TryGetProperty("type", out var t) && t.ValueKind == System.Text.Json.JsonValueKind.String)
                                        {
                                            code = t.GetString();
                                        }

                                        if (!string.IsNullOrEmpty(locStr) && !string.IsNullOrEmpty(msg))
                                        {
                                            detailItems.Add($"{locStr}: {msg}");
                                        }
                                        else if (!string.IsNullOrEmpty(msg))
                                        {
                                            detailItems.Add(msg!);
                                        }
                                        isFirst = false;
                                    }

                                    if (detailItems.Count > 0)
                                    {
                                        message = string.Join("; ", detailItems);
                                    }
                                }
                                else if (detailProp.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    message = detailProp.GetString() ?? message;
                                }
                            }
                            else if (root.TryGetProperty("message", out var directMsg) && directMsg.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                message = directMsg.GetString() ?? message;
                            }
                        }
                    }
                    catch
                    {
                        message = errorBody!;
                    }
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new BadRequestException(message, errorBody, param, code, errorType);
                    case HttpStatusCode.Unauthorized:
                        throw new AuthenticationException(message, errorBody, param, code, errorType);
                    case HttpStatusCode.Forbidden:
                        throw new PermissionDeniedException(message, errorBody, param, code, errorType);
                    case HttpStatusCode.NotFound:
                        throw new NotFoundException(message, errorBody, param, code, errorType);
                    case (HttpStatusCode)422:
                        throw new UnprocessableEntityException(message, errorBody, param, code, errorType);
                    case (HttpStatusCode)429:
                        throw new RateLimitException(message, errorBody, param, code, errorType);
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        throw new InternalServerException(message, response.StatusCode, errorBody, param, code, errorType);
                    default:
                        throw new ApiException(message, response.StatusCode, errorBody, param, code, errorType);
                }
            }
        }

        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _httpClient?.Dispose();
            }
        }
    }
}
