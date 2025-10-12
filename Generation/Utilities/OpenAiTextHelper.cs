// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Genova.Generation.Models;

namespace Genova.Generation.Utilities;

/// <summary>
/// Provides helper methods for working with text generation requests and responses to the OpenAI API.
/// </summary>
internal sealed class OpenAiTextHelper : OpenAiBaseHelper<OpenAiTextRequest, OpenAiTextResponse>
{
    /// <summary>
    /// Extracts the 'content' string from the GPT response, or returns null if it cannot be found.
    /// </summary>
    /// <param name="rootElement">The root element of the GPT API JSON document.</param>
    /// <returns>The GPT content string if present, or null otherwise.</returns>
    internal static string? ExtractContent(JsonElement rootElement)
    {
        if (!rootElement.TryGetProperty("choices", out JsonElement choices))
        {
            return null;
        }

        if (choices.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        if (choices.GetArrayLength() == 0)
        {
            return null;
        }

        if (!choices[0].TryGetProperty("message", out JsonElement message))
        {
            return null;
        }

        if (!message.TryGetProperty("content", out JsonElement contentElement))
        {
            return null;
        }

        return contentElement.GetString();
    }

    /// <inheritdoc/>
    protected override Dictionary<string, object?> BuildPayload(OpenAiTextRequest request)
    {
        return new Dictionary<string, object?>
        {
            ["model"] = request.Model ?? string.Empty,
            ["messages"] = new[]
            {
                new { role = "system", content = request.Context },
                new { role = "user", content = request.Prompt },
            },
        };
    }

    /// <inheritdoc/>
    protected override OpenAiTextResponse CreateErrorResponse(string error, int statusCode = -1)
    {
        OpenAiTextResponse response = CreateCommonErrorResponse(error, statusCode);
        response.Content = string.Empty;
        return response;
    }

    /// <inheritdoc/>
    protected override OpenAiTextResponse DeserializeFromRoot(JsonElement rootElement)
    {
        string? content = ExtractContent(rootElement);

        string? model = null;
        if (rootElement.TryGetProperty("model", out JsonElement modelElement))
        {
            model = modelElement.GetString();
        }

        return new OpenAiTextResponse
        {
            Success = true,
            Model = model ?? string.Empty,
            Content = content ?? string.Empty,
        };
    }
}
