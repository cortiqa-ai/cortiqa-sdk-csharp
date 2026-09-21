using System;
using System.Net;

namespace Cortiqa.Sdk.Exceptions
{
    /// <summary>
    /// Base exception for all Cortiqa SDK errors.
    /// </summary>
    public class CortiqaException : Exception
    {
        public CortiqaException(string message) : base(message) { }
        public CortiqaException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception representing an HTTP error returned by the Cortiqa API.
    /// </summary>
    public class ApiException : CortiqaException
    {
        public HttpStatusCode StatusCode { get; }
        public string? ResponseBody { get; }

        public ApiException(string message, HttpStatusCode statusCode, string? responseBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public ApiException(string message, HttpStatusCode statusCode, string? responseBody, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }

    /// <summary>
    /// Thrown when the API key is missing, invalid, or unauthorized (HTTP 401).
    /// </summary>
    public class AuthenticationException : ApiException
    {
        public AuthenticationException(string message, string? responseBody = null)
            : base(message, HttpStatusCode.Unauthorized, responseBody) { }
    }

    /// <summary>
    /// Thrown when request rate limits or concurrency quotas are exceeded (HTTP 429).
    /// </summary>
    public class RateLimitException : ApiException
    {
        public RateLimitException(string message, string? responseBody = null)
            : base(message, (HttpStatusCode)429, responseBody) { }
    }

    /// <summary>
    /// Thrown when a requested resource or model is not found (HTTP 404).
    /// </summary>
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message, string? responseBody = null)
            : base(message, HttpStatusCode.NotFound, responseBody) { }
    }

    /// <summary>
    /// Thrown when Cortiqa servers encounter an unexpected internal error (HTTP 500+).
    /// </summary>
    public class InternalServerException : ApiException
    {
        public InternalServerException(string message, HttpStatusCode statusCode, string? responseBody = null)
            : base(message, statusCode, responseBody) { }
    }
}
