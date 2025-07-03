// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Genova.Common.Utilities;
using Genova.Generation.Models;

namespace Genova.Generation.Utilities;

/// <summary>
/// Provides helper methods for working with image generation requests and responses to the OpenAI API.
/// </summary>
internal sealed class OpenAiImageHelper : OpenAiBaseHelper<OpenAiImageRequest, OpenAiImageResponse>
{
    /// <summary>
    /// Deserializes an OpenAI image response from the new (internal) JSON format, which contains a <c>b64_json</c>
    /// property in the <c>data</c> array and an <c>output_format</c> property at the root.
    /// Extracts the image bytes and determines the image format.
    /// </summary>
    /// <param name="rootElement">The root <see cref="JsonElement"/> of the OpenAI API response.</param>
    /// <returns>
    /// An <see cref="OpenAiImageResponse"/> populated with the image bytes and format.
    /// The <c>Url</c> property will be empty.
    /// </returns>
    internal static OpenAiImageResponse DeserializeFromInternalPayload(JsonElement rootElement)
    {
        byte[] bytes = ExtractImageBytes(rootElement);
        string format = ExtractFormat(rootElement);

        return new OpenAiImageResponse
        {
            Success = true,
            Model = string.Empty,
            Url = string.Empty,
            Bytes = bytes,
            Error = string.Empty,
            Format = format,
        };
    }

    /// <summary>
    /// Deserializes an OpenAI image response from the legacy (external) JSON format, which contains a
    /// <c>url</c> property in the <c>data</c> array. Extracts the image URL and determines the
    /// image format from the file extension.
    /// </summary>
    /// <param name="rootElement">The root <see cref="JsonElement"/> of the OpenAI API response.</param>
    /// <returns>
    /// An <see cref="OpenAiImageResponse"/> populated with the image URL and format.
    /// The <c>Bytes</c> property will be empty.
    /// </returns>
    internal static OpenAiImageResponse DeserializeFromExternalPayload(JsonElement rootElement)
    {
        string? url = ExtractUrl(rootElement);
        string format = string.Empty;

        if (!string.IsNullOrEmpty(url))
        {
            string ext = FileHelper.GetFileExtension(url);
            format = ext.StartsWith('.') ? ext.Substring(1).ToLowerInvariant() : ext.ToLowerInvariant();
        }

        return new OpenAiImageResponse
        {
            Success = true,
            Model = string.Empty,
            Url = url ?? string.Empty,
            Bytes = [],
            Error = string.Empty,
            Format = format,
        };
    }

    /// <summary>
    /// Extracts the 'url' from the first element of the data array, if available.
    /// Otherwise, returns null.
    /// </summary>
    /// <param name="root">The root JSON element from the GPT API response.</param>
    /// <returns>The image URL or null if not found.</returns>
    internal static string? ExtractUrl(JsonElement root)
    {
        if (!root.TryGetProperty("data", out JsonElement dataArray))
        {
            return null;
        }

        if (dataArray.ValueKind != JsonValueKind.Array || dataArray.GetArrayLength() == 0)
        {
            return null;
        }

        JsonElement firstData = dataArray[0];
        if (!firstData.TryGetProperty("url", out JsonElement urlElement))
        {
            return null;
        }

        if (urlElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return urlElement.GetString();
    }

    /// <inheritdoc/>
    protected override OpenAiImageResponse DeserializeFromRoot(JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("error", out JsonElement errorObj))
        {
            string? message = null;
            if (errorObj.TryGetProperty("message", out JsonElement messageElement))
            {
                message = messageElement.GetString();
            }

            return new OpenAiImageResponse
            {
                Success = false,
                Error = message ?? "Unknown error returned by GPT.",
                Model = string.Empty,
                Url = string.Empty,
                Format = string.Empty,
            };
        }

        if (IsInternalPayload(rootElement))
        {
            return DeserializeFromInternalPayload(rootElement);
        }
        else
        {
            return DeserializeFromExternalPayload(rootElement);
        }
    }

    /// <inheritdoc/>
    protected override Dictionary<string, object> BuildPayload(OpenAiImageRequest request)
    {
        return new Dictionary<string, object>
        {
            ["model"] = request.Model ?? string.Empty,
            ["prompt"] = request.Prompt,
            ["n"] = request.Number,
            ["size"] = request.Size,
        };
    }

    /// <inheritdoc/>
    protected override OpenAiImageResponse CreateErrorResponse(string error, int statusCode = -1)
    {
        OpenAiImageResponse response = CreateCommonErrorResponse(error, statusCode);
        response.Url = string.Empty;
        response.Format = string.Empty;
        response.Bytes = [];
        return response;
    }

    /// <summary>
    /// Determines if the payload is in the new (internal) format, i.e., contains "b64_json".
    /// </summary>
    private static bool IsInternalPayload(JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("data", out JsonElement dataArray) &&
            dataArray.ValueKind == JsonValueKind.Array &&
            dataArray.GetArrayLength() > 0)
        {
            JsonElement firstData = dataArray[0];
            return firstData.TryGetProperty("b64_json", out _);
        }

        return false;
    }

    private static byte[] ExtractImageBytes(JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("data", out JsonElement dataArray) &&
            dataArray.ValueKind == JsonValueKind.Array &&
            dataArray.GetArrayLength() > 0)
        {
            JsonElement firstData = dataArray[0];
            if (firstData.TryGetProperty("b64_json", out JsonElement b64Element) &&
                b64Element.ValueKind == JsonValueKind.String)
            {
                string? b64 = b64Element.GetString();
                if (!string.IsNullOrEmpty(b64))
                {
                    try
                    {
                        return Convert.FromBase64String(b64);
                    }
                    catch (FormatException)
                    {
                        // If base64 is invalid, return empty array
                    }
                }
            }
        }

        return [];
    }

    private static string ExtractFormat(JsonElement rootElement)
    {
        if (rootElement.TryGetProperty("output_format", out JsonElement formatElement) &&
            formatElement.ValueKind == JsonValueKind.String)
        {
            string? outputFormat = formatElement.GetString();
            if (!string.IsNullOrWhiteSpace(outputFormat))
            {
                return outputFormat.Trim().TrimStart('.').ToLowerInvariant();
            }
        }

        return string.Empty;
    }
}
