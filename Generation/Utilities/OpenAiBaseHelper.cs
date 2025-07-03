// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Genova.Generation.Models;

namespace Genova.Generation.Utilities;

/// <summary>
/// Provides a template base class for OpenAI API helpers, encapsulating common serialization and deserialization logic.
/// </summary>
/// <typeparam name="TRequest">The type representing the API request.</typeparam>
/// <typeparam name="TResponse">The type representing the API response.</typeparam>
internal abstract class OpenAiBaseHelper<TRequest, TResponse>
    where TRequest : IOpenAiRequest
    where TResponse : IOpenAiResponse
{
    /// <summary>
    /// Extracts rate limit information from the specified <see cref="HttpResponseMessage"/> and returns an <see cref="OpenAIRateLimits"/> object.
    /// </summary>
    /// <param name="httpResponse">The HTTP response message from which to extract rate limit headers.</param>
    /// <returns>An <see cref="OpenAIRateLimits"/> object containing the rate limit information.</returns>
    internal static OpenAIRateLimits GetRateLimits(HttpResponseMessage? httpResponse)
    {
        OpenAIRateLimits rateLimits = new();

        if (httpResponse == null)
        {
            return rateLimits;
        }

        HttpResponseHeaders? headers = httpResponse.Headers;
        HttpContentHeaders? contentHeaders = httpResponse.Content.Headers;

        string? limitRequests = GetHeader(headers, contentHeaders, "X-RateLimit-Limit-Requests");
        string? limitTokens = GetHeader(headers, contentHeaders, "X-RateLimit-Limit-Tokens");
        string? remainingRequests = GetHeader(headers, contentHeaders, "X-RateLimit-Remaining-Requests");
        string? remainingTokens = GetHeader(headers, contentHeaders, "X-RateLimit-Remaining-Tokens");
        string? resetRequests = GetHeader(headers, contentHeaders, "X-RateLimit-Reset-Requests");
        string? resetTokens = GetHeader(headers, contentHeaders, "X-RateLimit-Reset-Tokens");

        if (int.TryParse(limitRequests, out int limitRequestsValue))
        {
            rateLimits.LimitRequests = limitRequestsValue;
        }

        if (int.TryParse(limitTokens, out int limitTokensValue))
        {
            rateLimits.LimitTokens = limitTokensValue;
        }

        if (int.TryParse(remainingRequests, out int remainingRequestsValue))
        {
            rateLimits.RemainingRequests = remainingRequestsValue;
        }

        if (int.TryParse(remainingTokens, out int remainingTokensValue))
        {
            rateLimits.RemainingTokens = remainingTokensValue;
        }

        if (long.TryParse(resetRequests, out long resetRequestsUnix))
        {
            rateLimits.ResetRequests = DateTimeOffset.FromUnixTimeSeconds(resetRequestsUnix).UtcDateTime;
        }

        if (long.TryParse(resetTokens, out long resetTokensUnix))
        {
            rateLimits.ResetTokens = DateTimeOffset.FromUnixTimeSeconds(resetTokensUnix).UtcDateTime;
        }

        return rateLimits;
    }

    /// <summary>
    /// Returns a string value, or a default value if the input is null.
    /// </summary>
    /// <param name="value">The value to return if valid.</param>
    /// <param name="defaultValue">The value to return by default.</param>
    /// <returns>Either the value or the default, whichever is not null.</returns>
    internal static string GetStringValue(string? value, string defaultValue = "")
    {
        return value ?? defaultValue;
    }

    /// <summary>
    /// Retrieves the value of the specified HTTP header from the given <see cref="HttpHeaders"/> collection.
    /// </summary>
    /// <param name="headers">The <see cref="HttpHeaders"/> collection to search for the header value. May be <c>null</c>.</param>
    /// <param name="key">The name of the HTTP header to retrieve.</param>
    /// <returns>
    /// The first value associated with the specified header key if found; otherwise, <c>null</c>.
    /// </returns>
    internal static string? GetHeader(HttpHeaders? headers, string key)
    {
        if (headers == null)
        {
            return null;
        }

        if (headers.TryGetValues(key, out IEnumerable<string>? values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Serializes a request object into a JSON string suitable for the OpenAI API.
    /// </summary>
    /// <param name="request">The request object to serialize.</param>
    /// <returns>A JSON string representing the request.</returns>
    internal string Serialize(TRequest request)
    {
        Dictionary<string, object> payload = BuildPayload(request);
        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Deserializes an <see cref="HttpResponseMessage"/> from the OpenAI API into a response object.
    /// Handles unsuccessful responses and exceptions by returning an error response.
    /// </summary>
    /// <param name="httpResponse">The HTTP response message from the OpenAI API.</param>
    /// <returns>A response object representing the result.</returns>
    internal async Task<TResponse> Deserialize(HttpResponseMessage httpResponse)
    {
        TResponse response;

        if (httpResponse == null)
        {
            response = CreateErrorResponse("Response is null.");
        }
        else
        {
            string responseBody = string.Empty;
            try
            {
                responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    string reason = GetStringValue(httpResponse.ReasonPhrase, "Unknown error");
                    response = CreateErrorResponse(
                        $"HTTP {(int)httpResponse.StatusCode}: {reason}\n{responseBody}", (int)httpResponse.StatusCode);
                }
                else
                {
                    response = Deserialize(responseBody);
                }
            }
            catch (Exception exception)
            {
                response = CreateErrorResponse($"{exception.GetType().Name}: {exception.Message}\n{responseBody}");
            }
        }

        response.RateLimits = GetRateLimits(httpResponse);

        return response;
    }

    /// <summary>
    /// Deserializes a JSON string from the OpenAI API into a response object.
    /// Handles invalid or empty JSON by returning an error response.
    /// </summary>
    /// <param name="responseBody">The JSON response body from the OpenAI API.</param>
    /// <returns>A response object representing the result.</returns>
    internal TResponse Deserialize(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return CreateErrorResponse("Response body is null or empty.");
        }

        try
        {
            JsonDocument? jsonDocument = JsonSerializer.Deserialize<JsonDocument>(responseBody);
            return Deserialize(jsonDocument);
        }
        catch (Exception exception)
        {
            return CreateErrorResponse($"{exception.GetType().Name}: {exception.Message}\n{responseBody}");
        }
    }

    /// <summary>
    /// Deserializes a <see cref="JsonDocument"/> from the OpenAI API into a response object.
    /// Handles null documents by returning an error response.
    /// </summary>
    /// <param name="jsonDocument">The JSON document from the OpenAI API.</param>
    /// <returns>A response object representing the result.</returns>
    internal TResponse Deserialize(JsonDocument? jsonDocument)
    {
        if (jsonDocument == null)
        {
            return CreateErrorResponse("The response JSON is null.");
        }

        return Deserialize(jsonDocument.RootElement);
    }

    /// <summary>
    /// Deserializes a <see cref="JsonElement"/> from the OpenAI API into a response object.
    /// Handles undefined or null elements by returning an error response.
    /// </summary>
    /// <param name="rootElement">The root JSON element from the OpenAI API response.</param>
    /// <returns>A response object representing the result.</returns>
    internal TResponse Deserialize(JsonElement rootElement)
    {
        if (rootElement.ValueKind == JsonValueKind.Undefined)
        {
            return CreateErrorResponse("The RootElement is undefined.");
        }

        if (rootElement.ValueKind == JsonValueKind.Null)
        {
            return CreateErrorResponse("The RootElement is null.");
        }

        return DeserializeFromRoot(rootElement);
    }

    /// <summary>
    /// Creates a response object representing a failed OpenAI API operation with the specified error message and
    /// status code. This method sets <c>Success</c> to <c>false</c>, assigns the provided <paramref name="error"/>
    /// and <paramref name="statusCode"/>, and sets <c>Model</c> to an empty string. Derived classes may extend
    /// the returned response with additional properties as needed.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="statusCode">The HTTP status code associated with the error. Defaults to -1.</param>
    /// <returns>
    /// An instance of <typeparamref name="TResponse"/> with error details populated
    /// and <c>Success</c> set to <c>false</c>.
    /// </returns>
    protected static TResponse CreateCommonErrorResponse(string error, int statusCode = -1)
    {
        TResponse response = Activator.CreateInstance<TResponse>();
        response.Success = false;
        response.Error = error;
        response.StatusCode = statusCode;
        response.Model = string.Empty;
        return response;
    }

    /// <summary>
    /// Builds the payload dictionary for serialization from the request object.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <returns>A dictionary representing the payload.</returns>
    protected abstract Dictionary<string, object> BuildPayload(TRequest request);

    /// <summary>
    /// Creates an error response object with the specified error message.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A response object representing the error.</returns>
    protected abstract TResponse CreateErrorResponse(string error, int statusCode = -1);

    /// <summary>
    /// Deserializes the root <see cref="JsonElement"/> into a response object.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <param name="rootElement">The root JSON element from the OpenAI API response.</param>
    /// <returns>A response object representing the result.</returns>
    protected abstract TResponse DeserializeFromRoot(JsonElement rootElement);

    private static string? GetHeader(HttpHeaders? headers, HttpHeaders? contentHeaders, string key)
    {
        return GetHeader(headers, key) ?? GetHeader(contentHeaders, key);
    }
}
