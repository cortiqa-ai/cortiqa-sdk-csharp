using System;
using System.Net.Http;

namespace Cortiqa.Sdk
{
    /// <summary>
    /// Configuration options for initializing the Cortiqa client.
    /// </summary>
    public class CortiqaClientOptions
    {
        public const string DefaultBaseUrl = "https://api.cortiqa.co";
        public const int DefaultMaxRetries = 2;
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Your Cortiqa API key. Defaults to the CORTIQA_API_KEY environment variable.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Base URL for the Cortiqa API. Defaults to https://api.cortiqa.co.
        /// </summary>
        public string BaseUrl { get; set; } = DefaultBaseUrl;

        /// <summary>
        /// Request timeout. Defaults to 60 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; } = DefaultTimeout;

        /// <summary>
        /// Maximum retry attempts on 429 rate limits and 5xx server errors. Defaults to 2.
        /// </summary>
        public int MaxRetries { get; set; } = DefaultMaxRetries;

        /// <summary>
        /// Custom HttpClient instance. If null, a shared instance will be used.
        /// </summary>
        public HttpClient? HttpClient { get; set; }

        public static CortiqaClientOptions FromEnvironment()
        {
            return new CortiqaClientOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("CORTIQA_API_KEY"),
                BaseUrl = Environment.GetEnvironmentVariable("CORTIQA_BASE_URL") ?? DefaultBaseUrl
            };
        }
    }
}
