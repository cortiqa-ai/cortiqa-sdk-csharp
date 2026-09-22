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
        public string? Param { get; }
        public string? Code { get; }
        public string? ErrorType { get; }

        public ApiException(
            string message,
            HttpStatusCode statusCode,
            string? responseBody = null,
            string? param = null,
            string? code = null,
            string? errorType = null)
            : base(FormatMessage(message, param))
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            Param = param;
            Code = code;
            ErrorType = errorType;
        }

        public ApiException(
            string message,
            HttpStatusCode statusCode,
            string? responseBody,
            Exception innerException,
            string? param = null,
            string? code = null,
            string? errorType = null)
            : base(FormatMessage(message, param), innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            Param = param;
            Code = code;
            ErrorType = errorType;
        }

        private static string FormatMessage(string message, string? param)
        {
            if (!string.IsNullOrEmpty(param) && !message.Contains(param))
            {
                return $"[{param}] {message}";
            }
            return message;
        }
    }

    /// <summary>
    /// Thrown when request parameters are invalid or malformed (HTTP 400).
    /// </summary>
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, HttpStatusCode.BadRequest, responseBody, param, code, errorType) { }
    }

    /// <summary>
    /// Thrown when the API key is missing, invalid, or unauthorized (HTTP 401).
    /// </summary>
    public class AuthenticationException : ApiException
    {
        public AuthenticationException(string message, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, HttpStatusCode.Unauthorized, responseBody, param, code, errorType) { }
    }

    /// <summary>
    /// Thrown when access to a resource is forbidden (HTTP 403).
    /// </summary>
    public class PermissionDeniedException : ApiException
    {
        public PermissionDeniedException(string message, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, HttpStatusCode.Forbidden, responseBody, param, code, errorType) { }
    }

    /// <summary>
    /// Thrown when a requested resource or model is not found (HTTP 404).
    /// </summary>
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, HttpStatusCode.NotFound, responseBody, param, code, errorType) { }
    }

    /// <summary>
    /// Thrown when request parameter validation fails (HTTP 422).
    /// </summary>
    public class UnprocessableEntityException : ApiException
    {
        public UnprocessableEntityException(string message, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, (HttpStatusCode)422, responseBody, param, code, errorType) { }
    }

    /// <summary>
    /// Thrown when request rate limits or concurrency quotas are exceeded (HTTP 429).
    /// </summary>
    public class RateLimitException : ApiException
    {
        public RateLimitException(string message, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, (HttpStatusCode)429, responseBody, param, code, errorType) { }
    }

    /// <summary>
    /// Thrown when Cortiqa servers encounter an unexpected internal error (HTTP 500+).
    /// </summary>
    public class InternalServerException : ApiException
    {
        public InternalServerException(string message, HttpStatusCode statusCode, string? responseBody = null, string? param = null, string? code = null, string? errorType = null)
            : base(message, statusCode, responseBody, param, code, errorType) { }
    }
}
