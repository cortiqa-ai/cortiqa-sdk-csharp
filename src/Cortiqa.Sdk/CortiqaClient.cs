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

        public const string Version = "0.1.0";

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

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new AuthenticationException("Invalid or missing Cortiqa API key (HTTP 401).", errorBody);
                    case (HttpStatusCode)429:
                        throw new RateLimitException("Cortiqa rate limit exceeded (HTTP 429).", errorBody);
                    case HttpStatusCode.NotFound:
                        throw new NotFoundException($"Resource not found at {relativePath} (HTTP 404).", errorBody);
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        throw new InternalServerException($"Cortiqa server error (HTTP {statusCode}).", response.StatusCode, errorBody);
                    default:
                        throw new ApiException($"Cortiqa API error (HTTP {statusCode}): {errorBody}", response.StatusCode, errorBody);
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
